// const dotenv = require('dotenv').config({ path: '../.env' })
const keypress = require('keypress')
const { NearScanner } = require('@toio/scanner')

const DURATION = 700 // ms
const SPEED = {
  forward: [70, 70],
  backward: [-70, -70],
  left: [30, 70],
  right: [70, 30],
}

async function main() {
  console.log('start')
  // start a scanner to find nearest cube
  const num = 1
  const toios = await new NearScanner(num).start()

  // connect to the cube

  const cubes = []
  for (let i = 0; i < num; i++) {
    const cube = await toios[i].connect()
    cubes.push(cube)
  }
  console.log(cubes[0].id)

  keypress(process.stdin)
  process.stdin.on('keypress', (ch, key) => {
    // ctrl+c or q -> exit process
    if ((key && key.ctrl && key.name === 'c') || (key && key.name === 'q')) {
      process.exit()
    }

    switch (key.name) {
      case 'up':
        for (let i = 0; i < num; i++) {
          cubes[i].move(...SPEED.forward, DURATION)
        }
        break
      case 'down':
        for (let i = 0; i < num; i++) {
          cubes[i].move(...SPEED.backward, DURATION)
        }
        break
      case 'left':
        for (let i = 0; i < num; i++) {
          cubes[i].move(...SPEED.left, DURATION)
        }
        break
      case 'right':
        for (let i = 0; i < num; i++) {
          cubes[i].move(...SPEED.right, DURATION)
        }
        break
    }
  })

  process.stdin.setRawMode(true)
  process.stdin.resume()
}

main()
