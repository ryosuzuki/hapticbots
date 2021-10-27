const fs = require('fs')
const config = require('config')

class Reel {
  constructor() {
    this.io = null
    this.current
    this.min = 0
    this.max = 6000
    this.delta = 100
    this.ms = 300
    this.port = 8883
    this.node_ip = config.get('node_ip')
    this.reel_ips = config.get('reel_ips')
    this.toio_ids = config.get('toio_ids')

    this.current = []
    for (let i = 0; i < this.reel_ips.length; i++) {
      let ip = this.reel_ips[i]
      let id = this.toio_ids[i]
      this.current.push({
        num_id: i,
        reel_ip: ip,
        toio_id: id,
        height_a: 0,
        height_b: 0,
      })
    }

    this.prev = {}
  }

  init() {

    this.io.on('connection', (socket) => {
      console.log('socket-reel connected')

      socket.emit('init', {
        reel_ips: this.reel_ips,
        min: this.min,
        max: this.max,
        delta: this.delta,
      })

      // socket.on('target-ip', (data) => {
      //   this.reel_ip = data
      //   console.log(this.reel_ips[this.robot_id])
      // })

      socket.on('reel-calibrate', (data) => {
        data = this.parseJson(data)
        this.calibrate(data)
      })

      socket.on('reel-move', (data) => {
        data = this.parseJson(data)
        this.move(data)
      })

      socket.on('reel-up', (data) => {
        data = this.parseJson(data)
        this.up(data)
        // this.manualUp(data)
      })

      socket.on('reel-down', (data) => {
        data = this.parseJson(data)
        this.down(data)
        // this.manualDown(data)
      })

      socket.on('reel-left-down', (data) => {
        data = this.parseJson(data)
        this.manualLeftDown(data)
      })

      socket.on('reel-right-down', (data) => {
        data = this.parseJson(data)
        this.manualRightDown(data)
        // this.manualDown(data)
      })

      socket.on('reel-stop', (data) => {
        data = this.parseJson(data)
        this.stop(data)
      })

    })

    this.receiveCommand()
  }

  receiveCommand() {
    const dgram = require('dgram')
    const receiver = dgram.createSocket('udp4')

    receiver.on('message', (message, remote) => {
      message = message.toString()
      // message = 'ip: 10.0.0.61 , reel_a: 21 , reel_b: -10'
      let ip = message.split(' ')[1]
      let height_a = parseInt(message.split(' ')[4])
      let height_b = parseInt(message.split(' ')[7])

      let index = this.reel_ips.findIndex(reel_ip => {
        return reel_ip === ip
      })
      if (!this.current[index]) return false
      if (this.current[index].height_b != height_b) {
        // console.log(this.current)
        // console.log(height_b - this.current[index].height_b)
        console.log(height_b, height_b - this.current[index].height_b)
      }
      this.current[index].height_a = height_a
      this.current[index].height_b = height_b
      // console.log(this.current)
      // this.io.sockets.emit('current', this.current)
      // }
    })

    receiver.on('error', (err) => {
      console.log('ignore reel actuation')
    })

    receiver.bind(this.port, this.node_ip)
  }

  calibrate(data) {
    let id = data.id
    let ip = this.current[id].reel_ip
    console.log('send init')
    this.sendCommand(ip, { init: true })
  }

  move(data) {
    let id = data.id
    let ip = this.current[id].reel_ip
    let target = data.target
    console.log('move', data)
    if (!target) return
    if (target < this.min) return
    if (target > this.max) return

    const prev = this.prev[id]
    if (prev && prev-50 < target && target < prev+50) return

    let target_a = target
    let target_b = target
    if (data.tilt) {
      target_a = target + 1000
      target_b = target
    }
    this.sendCommand(ip, {
      move: true,
      target_a: target_a,
      target_b: target_b,
    })
    this.prev[id] = target
  }

  up(data) {
    let id = data.id
    let ip = this.current[id].reel_ip
    let delta = data.delta ? data.delta : this.delta
    let current_a = this.current[id].height_a
    let current_b = this.current[id].height_b
    let target_a = current_a + delta
    let target_b = current_b + delta
    console.log('current_a: ' + current_a + ' - up to ' + target_a)
    console.log('current_a: ' + current_b + ' - up to ' + target_b)
    this.sendCommand(ip, {
      move: true,
      target_a: target_a,
      target_b: target_b,
    })
  }

  down(data) {
    let id = data.id
    let ip = this.current[id].reel_ip
    let delta = data.delta ? data.delta : this.delta
    let current_a = this.current[id].height_a
    let current_b = this.current[id].height_b
    let target_a = current_a - delta
    let target_b = current_b - delta
    console.log('current_a: ' + current_a + ' - up to ' + target_a)
    console.log('current_a: ' + current_b + ' - up to ' + target_b)
    this.sendCommand(ip, {
      move: true,
      target_a: target_a,
      target_b: target_b,
    })
  }

  stop(data) {
    let id = data.id
    let ip = this.current[id].reel_ip
    console.log('send stop')
    this.sendCommand(ip, { stop: true })
  }

  manualUp(data) {
    let id = data.id
    let ip = this.current[id].reel_ip
    let ms = Math.round(this.ms * 0.8)
    this.sendCommand(ip, { a1: 1000, a2: 0, b1: 1000, b2: 0, ms: ms })
  }

  manualDown(data) {
    let id = data.id
    let ip = this.current[id].reel_ip
    this.sendCommand(ip, { a1: 0, a2: 1000, b1: 0, b2: 1000, ms: this.ms })
  }

  manualLeftDown(data) {
    let id = data.id
    let ip = this.current[id].reel_ip
    this.sendCommand(ip, { a1: 0, a2: 0, b1: 0, b2: 1000, ms: this.ms })
  }

  manualRightDown(data) {
    let id = data.id
    let ip = this.current[id].reel_ip
    this.sendCommand(ip, { a1: 0, a2: 0, b1: 1000, b2: 0, ms: this.ms })
  }

  parseJson(data) {
    try {
      data = JSON.parse(data)
    } catch (err) {
      return data
    }
    return data
  }

  sendCommand(ip, json) {
    const dgram = require('dgram')
    const client = dgram.createSocket('udp4')

    // console.log(json)
    let str = JSON.stringify(json)
    let message = Buffer.from(str)
    let reel_ip = this.reel_ips[this.robot_id]
    client.send(message, 0, message.length, this.port, ip, function(err, bytes) {
      if (err) throw err
      client.close()
    })
  }

}

module.exports = Reel