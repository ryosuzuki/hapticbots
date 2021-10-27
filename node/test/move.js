const dotenv = require('dotenv').config({ path: '../.env' })
const keypress = require('keypress')
const { NearestScanner } = require('@toio/scanner')

const DURATION = 700 // ms
const SPEED = {
  forward: [70, 70],
  backward: [-70, -70],
  left: [30, 70],
  right: [70, 30],
}

async function main() {
  // start a scanner to find nearest cube
  const cube = await new NearestScanner(2).start()

  // connect to the cube
  await cube.connect()

  keypress(process.stdin)
  process.stdin.on('keypress', (ch, key) => {
    // ctrl+c or q -> exit process
    if ((key && key.ctrl && key.name === 'c') || (key && key.name === 'q')) {
      process.exit()
    }

    switch (key.name) {
      case 'up':
        cube.move(...SPEED.forward, DURATION)
        break
      case 'down':
        cube.move(...SPEED.backward, DURATION)
        break
      case 'left':
        cube.move(...SPEED.left, DURATION)
        break
      case 'right':
        cube.move(...SPEED.right, DURATION)
        break
    }
  })

  process.stdin.setRawMode(true)
  process.stdin.resume()
}

main()