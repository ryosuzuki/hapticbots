const { NearScanner } = require('@toio/scanner')
const munkres = require('munkres-js')
const fs = require('fs')
const config = require('config')

class Toio {
  constructor() {
    this.num = config.get('total_num')
    this.robot_ids = config.get('toio_ids')
    this.robots = []
    this.targets = []
    this.sleepTime = 100
    this.io = null
    this.height

    this.count = 0
    this.prev = {}

    this.minecraft = 0
  }

  newTarget() {
    this.targets = []
    for (let i = 0; i < this.num; i++) {
      let offset = Math.PI / 8 * this.count
      let theta = 2 * Math.PI / this.num
      this.targets.push({
        x: 100 * Math.cos(theta * i + offset) + 250,
        y: 100 * Math.sin(theta * i + offset) + 250,
        angle: 0,
      })
      /*
      this.targets.push({
        x: Math.round(Math.random() * 350) + 50,
        y: Math.round(Math.random() * 350) + 50,
        angle: Math.round(Math.random() * 360),
      })
      */
    }
    this.count++
    console.log(this.targets)
  }

  async init() {
    console.log('init')

    this.io.on('connection', (socket) => {
      console.log('socket-toio connected')

      socket.on('minecraft', (data) => {
        const hoge = {
          id: this.minecraft % 3,
          target: 800,
          tilt: false,
        }
        this.minecraft = this.minecraft + 1
        this.io.sockets.emit('reel-move', hoge)
      })

      socket.on('move', (data) => {
        data = this.parseJson(data)
        // console.log(data)
        // robots[0].target = { x: data.x, y: data.y }
        if (data && data.x && data.y) {
          this.targets = [
            { x: data.x, y: data.y, angle: 0, tick: 0 }
          ]
        }
        if (data && data.robots) {
          let targets = data.robots // from unity
          /*
          let targets = []
          for (let pos of data.robots) {
            if (50 < pos.x && pos.x < 450 && 50 < pos.y && pos.y < 450) {
              targets.push(pos)
            }
          }
          */
          this.targets = targets
          this.io.sockets.emit('targets', this.targets )
        }
      })

      socket.on('calibrate', (data) => {
        console.log(data)
        // this.io.sockets.emit('calibrate', data)
        this.io.emit('calibrate', data)
      })

    })

    if (this.targets.length < this.num) {
      // this.newTarget()
      /*
      for (let i = 0 ; i < this.num; i++) {
        this.targets.push({
          x: Math.round(Math.random() * 200) + 100,
          y: Math.round(Math.random() * 200) + 100,
          angle: Math.round(Math.random() * 360),
        })
      }
      */
      // console.log(this.targets)
      this.targets = [
        { x: 350, y: 150, angle: 0 },
        // { x: 250, y: 350, angle: 0 },
        // { x: 150, y: 250, angle: 0 },
        // { x: 350, y: 250, angle: 0 }
      ]
    }


    // return

    this.robots = await new NearScanner(this.num).start()
    for (let i = 0; i < this.num; i++) {
      const robot = await this.robots[i].connect()
      let index = this.robot_ids.findIndex(robot_id => {
        return robot_id === robot.id
      })
      robot.numId = index
      console.log(index)
    }
    console.log('toio connected')
    console.log(this.targets)
    console.log(this.robots.map(e => {
      return { numId: e.numId, id: e.id }
    }))

    this.height = parseFloat(fs.readFileSync('height.txt').toString())

    for (let robot of this.robots) {
      robot.turnOnLight({ durationMs: 0, red: 255, green: 0, blue: 255 })
      robot.on('id:position-id', data => {
        robot.x = data.x
        robot.y = data.y
        robot.angle = data.angle
        // console.log(robot.numId)
        // console.log(robot.x, robot.y, robot.angle)

        if (this.io) {
          let robots = this.robots.map((e) => {
            return { id: e.id, numId: e.numId, x: e.x, y: e.y, angle: e.angle}
          })
          this.io.sockets.emit('pos', { robots: robots, targets: this.targets })
        }
      })
    }

    let count = 0
    while (true) {
      try {
        this.loop()
        /*
        count++
        if (count > 10) {
          count = 0
          this.newTarget()
        }
        */
      } catch (err) {
        console.log(err)
      }
      await this.sleep(this.sleepTime) // 100
    }
  }

  parseJson(data) {
    try {
      data = JSON.parse(data)
    } catch (err) {
      return data
    }
    return data
  }

  loop() {
    let res = this.assign()
    let distMatrix = res.distMatrix
    let rids = res.rids
    let ids = munkres(distMatrix)
    for (let id of ids) {
      let targetId = id[0]
      let numId = rids[id[1]]
      let target = this.targets[targetId]
      this.move(numId, target)
      let tilt = false
      if (target.angle) {
        tilt = true
      }
      const data = {
        id: numId,
        target: target.tick,
        tilt: tilt,
      }
      // if (data.target) {
      //   reel.move(data)
      // }
      if (!data.target) continue
      const prev = this.prev[numId]
      if (prev && prev-50 < data.target && data.target < prev+50) continue

      // For house
      /*
      if (numId === 0) {
        console.log('reel-move', data.target)
        data.target = 2000
        this.io.sockets.emit('reel-move', data)
      }
      */
      console.log('reel-move', data.target)
      // this.io.sockets.emit('reel-move', data)
      this.prev[numId] = data.target
    }
  }

  assign() {
    let distMatrix = []
    let rids = []
    for (let target of this.targets) {
      let distArray = []
      for (let robot of this.robots) {
        if (!robot.x || !robot.y) continue
        let dx = target.x - robot.x
        let dy = target.y - robot.y
        let dist = Math.sqrt(dx*dx + dy*dy)
        distArray.push(dist)
        rids.push(robot.numId)
      }
      distMatrix.push(distArray)
    }
    if (!distMatrix.length) return
    return { distMatrix: distMatrix, rids: rids }
  }

  getRobotByNumId(numId) {
    return this.robots.filter((robot) => {
      return robot.numId == numId
    })[0]
  }

  async move(numId, target) {
    if (!target.angle) {
      target.angle = 0
    }
    let status = this.calculate(numId, target)
    let distThreshold = 1
    let dirThreshold = 45

    let angleDiff = (360 + status.angleDiff) % 180
    // console.log(rvo, dirThreshold)

    let calc = this.getDirection(status.diff, dirThreshold)
    let dist = status.dist
    let dir = calc.dir
    let diff = calc.diff

    let command

    let speed = 80 // 150
    const ratio = 1 - Math.abs(diff) / 90
    let rot = 0.5
    if (dist < 60) {
      // speed = dist // > 15 ? dist : 15
      speed = dist**2 / 4
      speed = speed > 40 ? 40 : speed
      // rot = 0.05
      rot = 0.2
    }

    if (dist < distThreshold) {
      return false
    }

    /*
    let rot = 0.5
    if (dist < 60) {
      // speed = dist // > 15 ? dist : 15
      // speed = dist**2 / 4
      speed = dist**2 / 4
      speed = speed > 40 ? 40 : speed
      // rot = 0.05
      rot = 0.2
    }
    */

    let angleThreshold = 10
    if(target.angle != 0 && dist < 20){
      // return false // stop
      if (angleDiff < angleThreshold) {
        return false // stop
      } else {
        dir = 'right'
        speed = 30
        rot = 0.35
      }
      if (180 - angleDiff < angleThreshold) {
        return false // stop
      } else {
        dir = 'left'
        speed = 30
        rot = 0.35
      }
    }


     // let ratio = 1 - Math.abs(diff) / 90
    // console.log(diff)
    switch (dir) {
      case 'forward':
        if (diff > 0) {
          // slightly turn right
          command = { left: speed, right: speed * ratio}
        } else {
          // slightly turn left
          command = { left: speed * ratio, right: speed}
        }
        break
      case 'backward':
        if (diff < 0) {
          // slightly turn right
          command = { left: -speed * ratio, right: -speed }
        } else {
          // slightly turn left
          command = { left: -speed, right: -speed * ratio}
        }
        break
      case 'left':
        command = { left: -speed * rot, right: speed * rot }
        break
      case 'right':
        command = { left:  speed * rot, right: -speed * rot }
        break
    }
    /*
    console.log(angleDiff)
    console.log(dir)
    console.log(command)
    */

    const robot = this.getRobotByNumId(numId) //this.robots[id]
    robot.move(command.left, command.right, this.sleepTime)
  }

  getDirection(diff, threshold) {
    if (0 <= diff && diff < threshold) {
      return { dir: 'forward', diff: diff }
    }
    if (threshold <= diff && diff < 90) {
      return { dir: 'right', diff: diff }
    }
    if (90 <= diff && diff < 180 - threshold) {
      return { dir: 'left', diff: 180 - diff }
    }
    if (180 - threshold <= diff && diff < 180 + threshold) {
      return { dir: 'backward', diff: 180 - diff }
    }
    if (180 + threshold <= diff && diff < 270) {
      return { dir: 'right', diff: diff - 180 }
    }
    if (270 <= diff && diff < 360 - threshold) {
      return { dir: 'left', diff: 360 - diff }
    }
    if (360 - threshold <= diff && diff <= 360) {
      return { dir: 'forward', diff: diff - 360 }
    }
  }

  async sleep(time) {
    return new Promise((resolve, reject) => {
      setTimeout(resolve, time)
    })
  }

  calculate(numId, target) {
    // const robot = this.robots[id]
    const robot = this.getRobotByNumId(numId)
    let dx = target.x - robot.x
    let dy = target.y - robot.y
    let dist = Math.sqrt(dx**2 + dy**2)
    let angleDiff = target.angle - robot.angle

    let dir = Math.atan2(dx, dy) * 180 / Math.PI
    dir = (-dir + 180) % 360
    let diff = Math.min((360) - Math.abs(robot.angle - dir), Math.abs(robot.angle - dir))
    // Example
    // * 1 - 359 = -358 < 0 && 358 > 180 -> -2
    // * 1 - 180 = -179 < 0 && 179 < 180 -> +179
    // * 15 - 1  =  14  > 0 && 14  < 180 -> -14
    // * 1 - 200 = -199 < 0 && 199 > 180 -> -161
    // * 359 - 1 =  358 > 0 && 358 > 180 -> +2
    if (robot.angle - dir < 0 && Math.abs(robot.angle - dir) > 180) {
      diff = -diff
    }
    if (robot.angle - dir > 0 && Math.abs(robot.angle - dir) < 180) {
      diff = -diff
    }
    diff = (diff + 360 - 90) % 360

    // angleDiff = (angleDiff + 180) % 180
    return { dist: dist, diff: diff, angleDiff: angleDiff }
  }


  /*
    Not using RVO algorithm for now

    const dt = 1
    let rvo = this.getRvoVelocity(id, target, dt)
    { x: rvoVx, y: rvoVy, dist: dist, diff: diff, angleDiff: angleDiff }
  */

  getRvoVelocity(id, target, dt) {
    const acceleration = 1
    const avoidanceTendency = 10

    const accel = acceleration
    const w = avoidanceTendency

    const robot = this.robots[id]
    // const target = App.targets[id]
    // console.log(robot)
    if (!target) return { x: 0, y: 0 }

    let prefVx = target.x - robot.x
    let prefVy = target.y - robot.y
    const dist = Math.sqrt(prefVx**2 + prefVy**2)
    if (dist > 1) {
      prefVx *= robot.prefSpeed / dist
      prefVy *= robot.prefSpeed / dist
    }

    let rvoVx = prefVx
    let rvoVy = prefVy
    let min = Infinity

    /*
    console.log(rvoVx, rvoVy)
    for (let i = 0; i < 100; i++) {
      const vx = robot.velocity.x + accel * dt // * (2 * Math.random() - 1)
      const vy = robot.velocity.y + accel * dt // * (2 * Math.random() - 1)
      const collisionTime = this.getCollisionTime(id, vx, vy)
      const dvx = vx - prefVx
      const dvy = vy - prefVy
      const penalty = w / collisionTime + Math.sqrt(dvx**2 + dvy**2)
      if (penalty < min) {
        rvoVx = vx
        rvoVy = vy
        min = penalty
      }
    }
    console.log(rvoVx, rvoVy)
    */

    let dir = Math.atan2(rvoVx, rvoVy) * 180 / Math.PI
    dir = (-dir + 180) % 360
    let diff = Math.min((360) - Math.abs(robot.angle - dir), Math.abs(robot.angle - dir))
    // 1 - 359 = -358 < 0 && 358 > 180 -> -2
    // 1 - 180 = -179 < 0 && 179 < 180 -> +179
    // 15 - 1  =  14  > 0 && 14  < 180 -> -14
    // 1 - 200 = -199 < 0 && 199 > 180 -> -161
    // 359 - 1 =  358 > 0 && 358 > 180 -> +2
    if (robot.angle - dir < 0 && Math.abs(robot.angle - dir) > 180) {
      diff = -diff
    }
    if (robot.angle - dir > 0 && Math.abs(robot.angle - dir) < 180) {
      diff = -diff
    }
    diff = (diff + 360 - 90) % 360

    let angleDiff = target.angle - (robot.angle + 90)
    angleDiff = (angleDiff + 180) % 180
    return { x: rvoVx, y: rvoVy, dist: dist, diff: diff, angleDiff: angleDiff }
  }

  getCollisionTime(id, vx, vy) {
    let tmin = Infinity
    let a = this.robots[id]
    let max = App.state.robots.length
    for (let i = 1; i <= max; i++) {
      if (i === id) continue

      const b = this.robots[i]
      if (b === null) continue

      const ux = 2 * vx - a.velocity.x - b.velocity.x
      const uy = 2 * vy - a.velocity.y - b.velocity.y
      const dx = b.x - a.x
      const dy = b.y - a.y
      const s = a.size + b.size
      const c2 = ux * ux + uy * uy
      const c1 = -2 * (ux * dx + uy * dy)
      const c0 = dx * dx + dy * dy - s * s

      let t = Infinity;
      if (c2 == 0) {
        t = -c0 / c1
      } else {
        const discriminant = c1 * c1 - 4 * c2 * c0
        if (discriminant >= 0) {
          const sq = Math.sqrt(discriminant)
          const t1 = (-c1 - sq) / (2 * c2)
          const t2 = (-c1 + sq) / (2 * c2)
          if (c0 < 0) {
            t = 0;  // Already collided!
          } else if (c1 <= 0) {
            t = t1;
          }
        }
      }

      if (t < tmin) {
        tmin = t
      }
    }
    return tmin
  }

}

module.exports = Toio