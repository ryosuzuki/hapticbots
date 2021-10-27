const dotenv = require('dotenv').config()
const express = require('express')
const socketio = require('socket.io')
const http = require('http')
const path = require('path')
const app = express()
const server = http.Server(app)
const io = socketio(server)

const Toio = require('./libs/toio')
const toio = new Toio()

const Reel = require('./libs/reel')
const reel = new Reel()

app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname + '/index.html'))
})

app.get('/vr', (req, res) => {
  res.sendFile(path.join(__dirname + '/vr.html'))
})

server.listen(3000, () => {
  console.log('listening on 3000')
  // main()
  reel.io = io
  reel.init()

  toio.io = io
  toio.init()

})
