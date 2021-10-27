const keypress = require('keypress')
const { NearestScanner } = require('@toio/scanner')

let cube
let cubeX = 0
let cubeY = 0
let cubeAngle = 0

let targetX = 250
let targetY = 250

function move(x, y, cubeX, cubeY, cubeAngle) {
  // console.log(x, y, cubeX, cubeY, cubeAngle)

  const diffX = x - cubeX
  const diffY = y - cubeY
  const distance = Math.sqrt(diffX * diffX + diffY * diffY)
  let speed = 80
  if (distance < speed) {
    speed = distance > 15 ? distance : 15
  }
  if (distance < 5) {
    return [0, 0] // stop
  }

  let relAngle = (Math.atan2(diffY, diffX) * 180) / Math.PI - cubeAngle
  relAngle = relAngle % 360
  if (relAngle < -180) {
    relAngle += 360
  } else if (relAngle > 180) {
    relAngle -= 360
  }

  const ratio = 1 - Math.abs(relAngle) / 90
  if (relAngle > 0) {
    return [speed, speed * ratio]
  } else {
    return [speed * ratio, speed]
  }
}
  
async function main() {
  console.log('start')
  const cube = await new NearestScanner().start()
  await cube.connect()
  console.log('toio connected')

  cube.turnOnLight({ durationMs: 0, red: 255, green: 0, blue: 255 })
  cube.on('id:position-id', data => {
    cubeX = data.x
    cubeY = data.y
    cubeAngle = data.angle
    console.log(cubeX, cubeY, cubeAngle)
  })

  setInterval(() => {
    cube.move(...move(targetX, targetY, cubeX, cubeY, cubeAngle), 100)
  }, 50)

}

main()