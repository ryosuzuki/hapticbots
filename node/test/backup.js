const dotenv = require('dotenv').config()
const express = require('express')
const socketio = require('socket.io')
const http = require('http')
const path = require('path')
const app = express()
const server = http.Server(app)
const io = socketio(server)

const Control = require('./control')
const control = new Control()

app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname + '/index.html'))
})

server.listen(3000, () => {
  console.log('listening on 3000')
  // main()
  control.init()

})

// const { NearScanner } = require('@toio/scanner')
let robots

const num = 1
let targets = []
for (let i = 0; i < num; i++) {
  let x = Math.round(Math.random() * 300 + 100)
  let y = Math.round(Math.random() * 300 + 100)
  targets.push({ x: x, y: y })
}

async function main() {
  console.log('start')
  robots = await new NearScanner(num).start()
  for (let i = 0; i < num; i++) {
    const robot = await robots[i].connect()
    const target = targets[i]
    robot.numId = i
    robot.target = target
  }
  console.log('toio connected')


  control.robots = robots
  control.targets = targets
  /*
  for (let i = 0; i < num; i++) {
    const robot = robots[i]
    robot.turnOnLight({ durationMs: 0, red: 255, green: 0, blue: 255 })
    robot.on('id:position-id', data => {
      robot.x = data.x
      robot.y = data.y
      robot.angle = data.angle
      // console.log(robot.x, robot.y, robot.angle)
    })
  }

  setInterval(() => {
    for (let i = 0; i < num; i++) {
      const robot = robots[i]
      robot.move(...move(robot), 100)
    }
  }, 50)
  */
}

io.on('connection', (socket) => {
  console.log('socket connected')
  /*
  let max = 450
  let min = 50
  setInterval(() => {
    let x = Math.random() * (max - min) + min
    let y = Math.random() * (max - min) + min
    socket.emit('pos', { x: x, y: y, angle: 30 })
  }, 1000)
  */

  socket.on('move', (data) => {
    console.log(data)
    robots[0].target = { x: data.x, y: data.y }
  })

  socket.on('actuate', (data) => {
    console.log(data)
    actuate(data.dir)
  })
})

function move(robot) {
  // console.log(x, y, robotX, robotY, robotAngle)
  io.sockets.emit('pos', { numId: robot.numId, x: robot.x, y: robot.y, angle: robot.angle, target: robot.target })

  const diffX = robot.target.x - robot.x
  const diffY = robot.target.y - robot.y
  const distance = Math.sqrt(diffX * diffX + diffY * diffY)
  let speed = 35
  if (distance < speed) {
    speed = distance > 15 ? distance : 15
  }
  if (distance < 50) {
    return [0, 0] // stop
  }

  let relAngle = (Math.atan2(diffY, diffX) * 180) / Math.PI - robot.angle
  relAngle = relAngle % 360
  if (relAngle < -180) {
    relAngle += 360
  } else if (relAngle > 180) {
    relAngle -= 360
  }

  console.log(relAngle)

  const ratio = 1 - Math.abs(relAngle) / 90
  if (relAngle > 0) {
    return [speed, speed * ratio]
  } else {
    return [speed * ratio, speed]
  }
}


function actuate(dir) {
  const ip = '10.0.0.231'
  const dgram = require('dgram')
  const client = dgram.createSocket('udp4')

  const ms = 1000
  let json = {}
  if (dir === 'up') {
    json = { a1: 1000, a2: 0, b1: 1000, b2: 0, ms: ms } // up
  } else {
    json = { a1: 0, a2: 1000, b1: 0, b2: 1000, ms: ms } // down
  }
  let str = JSON.stringify(json)
  let message = Buffer.from(str)
  let port = 8883
  console.log(message)
  client.send(message, 0, message.length, port, ip, function(err, bytes) {
    if (err) throw err
    client.close()
  })
}