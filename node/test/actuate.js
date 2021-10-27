const config = require('config')
const reel_ips = config.get('reel_ips')
const test_id = 0
const ip = reel_ips[test_id]

console.log('start ' + ip)

const sendCommand = function(json) {
  const dgram = require('dgram')
  const client = dgram.createSocket('udp4')

  let str = JSON.stringify(json)
  let message = Buffer.from(str)
  let port = 8883
  console.log(message)
  client.send(message, 0, message.length, port, ip, function(err, bytes) {
    if (err) throw err
    client.close()
  })
}

// let json = { a1: 0, a2: 1000, ms: 1000 } // down
// json = { a1: 1000, a2: 0, ms: 1000 } // up

let ms = 1000
let json = { a1: 0, a2: 1000, b1: 0, b2: 1000, ms: ms } // down
json = { a1: 1000, a2: 0, b1: 1000, b2: 0, ms: ms } // up

// json = { a1: 0, a2: 1000, b1: 0, b2: 0, ms: ms } // right down
// json = { a1: 0, a2: 0, b1: 0, b2: 1000, ms: ms } // left down

sendCommand(json)

// setInterval(() => {
//   sendCommand(json)
// }, 3000)