const config = require('config')
const ip = config.get('node_ip')

console.log('start ' + ip)

const receiveCommand = function() {
  const dgram = require('dgram')
  const receiver = dgram.createSocket('udp4')

  receiver.on('message', (message, remote) => {
    let current = parseFloat(message.toString())
    console.log(current)
  })

  let port = 8883
  receiver.bind(port, ip)
}

receiveCommand()