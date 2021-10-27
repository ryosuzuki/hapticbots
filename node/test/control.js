const keypress = require('keypress')
const config = require('config')
const node_ip = config.get('node_ip')
const reel_ips = config.get('reel_ips')
const port = 8883

const test_id = 0
const reel_ip = reel_ips[test_id]
console.log('start')
console.log('press space key for force stop')

let current = -99
let delta = 100


function target() {
  const target = 200
  sendCommand({ move: true,
    target_a: target,
    target_b: target,
  })
}

function init() {
  console.log('send init')
  sendCommand({ init: true })
}

function up() {
  console.log('send up to ' + (current + delta))
  sendCommand({
    move: true,
    target_a: current + delta,
    target_b: current + delta,
  })
}

function down() {
  console.log('send down to ' + (current - delta))
  sendCommand({
    move: true,
    target_a: current - delta,
    target_b: current - delta,
  })
}

function stop() {
  console.log('send stop')
  sendCommand({ stop: true })
}

function manualUp() {
  sendCommand({ a1: 1000, a2: 0, b1: 1000, b2: 0, ms: 1000 })
}

function manualDown() {
  sendCommand({ a1: 0, a2: 1000, b1: 0, b2: 1000, ms: 1000 })
}

const sendCommand = function(json) {
  const dgram = require('dgram')
  const client = dgram.createSocket('udp4')

  // console.log(json)
  let str = JSON.stringify(json)
  let message = Buffer.from(str)
  client.send(message, 0, message.length, port, reel_ip, function(err, bytes) {
    if (err) throw err
    client.close()
  })
}

const receiveCommand = function() {
  const dgram = require('dgram')
  const receiver = dgram.createSocket('udp4')
  receiver.on('message', (message, remote) => {
    message = message.toString()
    // message = 'ip: 10.0.0.61 , reel_a: 21 , reel_b: -10'
    let ip = message.split(' ')[1]
    let height_a = parseInt(message.split(' ')[4])
    let height_b = parseInt(message.split(' ')[7])
    if (current != height_b) {
      current = height_b
      console.log(message)
    }
  })
  receiver.bind(port, node_ip)
}

receiveCommand()


keypress(process.stdin)
process.stdin.on('keypress', (ch, key) => {
  // ctrl+c or q -> exit process
  if ((key && key.ctrl && key.name === 'c') || (key && key.name === 'q')) {
    process.exit()
  }

  switch (key.name) {
    case 'up':
      up()
      break
    case 'down':
      down()
      break
    case 'space':
      stop()
      break
    case 'i':
      init()
      break
    case 't':
      target()
      break
  }
})

process.stdin.setRawMode(true)
process.stdin.resume()

// init()