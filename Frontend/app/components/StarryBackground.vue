<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref } from 'vue'

const canvasRef = ref<HTMLCanvasElement | null>(null)

let ctx!: CanvasRenderingContext2D
let stars: any[] = []
let shootingStars: any[] = []
let animationFrame = 0
let width = 0
let height = 0
let starRGB = '255,255,255'

let time = 0
let lastTime = 0

const STAR_COUNT = 160

function getStarColor() {
  const bg = getComputedStyle(document.documentElement)
    .getPropertyValue('--ui-bg')
    .trim()

  const match = bg.match(/(\d+)%\)$/)
  const lightness = match ? parseInt(match[1]) : 50
  const isDark = lightness < 50

  if (isDark) {
    starRGB = '255,170,80'
  } else {
    starRGB = '255,170,80'
  }
}

function resize() {
  width = window.innerWidth
  height = window.innerHeight
  const canvas = canvasRef.value
  if (!canvas) return
  canvas.width = width
  canvas.height = height
}

function createStars() {
  stars = []
  for (let i = 0; i < STAR_COUNT; i++) {
    stars.push({
      x: Math.random() * width,
      y: Math.random() * height,
      radius: 0.6 + Math.random() * 0.9,
      baseOpacity: 0.4 + Math.random() * 0.25,
      amplitude: 0.15 + Math.random() * 0.1,
      speed: 0.6 + Math.random() * 0.8,
      offset: Math.random() * Math.PI * 2
    })
  }
}

function createShootingStar() {
  const startX = Math.random() * width
  const startY = Math.random() * height * 0.4

  shootingStars.push({
    x: startX,
    y: startY,
    vx: 6 + Math.random() * 2,
    vy: 1.5 + Math.random(),
    life: 0,
    maxLife: 70
  })
}

function drawStars() {
  stars.forEach(star => {
    const shimmer =
      star.baseOpacity +
      Math.sin(time * star.speed + star.offset) *
        star.amplitude

    const opacity = Math.max(0.2, Math.min(1, shimmer))

    ctx.beginPath()
    ctx.arc(star.x, star.y, star.radius, 0, Math.PI * 2)
    ctx.fillStyle = `rgba(${starRGB}, ${opacity})`
    ctx.shadowBlur = 2 + opacity * 4
    ctx.shadowColor = `rgba(${starRGB}, ${opacity * 0.6})`
    ctx.fill()
    ctx.shadowBlur = 0
  })
}

function drawShootingStars() {
  shootingStars.forEach((star, i) => {
    const progress = star.life / star.maxLife
    const opacity = 1 - progress

    const tailLength = 180

    const gradient = ctx.createLinearGradient(
      star.x,
      star.y,
      star.x - tailLength,
      star.y - tailLength * 0.25
    )

    gradient.addColorStop(0, `rgba(${starRGB}, ${opacity})`)
    gradient.addColorStop(1, `rgba(${starRGB}, 0)`)

    ctx.beginPath()
    ctx.moveTo(star.x, star.y)
    ctx.lineTo(
      star.x - tailLength,
      star.y - tailLength * 0.25
    )
    ctx.strokeStyle = gradient
    ctx.lineWidth = 2
    ctx.shadowBlur = 6
    ctx.shadowColor = `rgba(${starRGB},0.6)`
    ctx.stroke()
    ctx.shadowBlur = 0

    star.x += star.vx
    star.y += star.vy
    star.life++

    if (star.life >= star.maxLife) {
      shootingStars.splice(i, 1)
    }
  })
}

function draw() {
  ctx.clearRect(0, 0, width, height)
  drawStars()
  drawShootingStars()
}

function animate(timestamp: number) {
  if (!lastTime) lastTime = timestamp
  const delta = (timestamp - lastTime) / 1000
  lastTime = timestamp

  time += delta

  draw()

  if (Math.random() < 0.0015) {
    createShootingStar()
  }

  animationFrame = requestAnimationFrame(animate)
}

onMounted(() => {
  const canvas = canvasRef.value
  if (!canvas) return

  const context = canvas.getContext('2d')
  if (!context) return

  ctx = context

  getStarColor()
  resize()
  createStars()
  animate(0)

  const observer = new MutationObserver(() => {
    getStarColor()
  })

  observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['class']
  })

  window.addEventListener('resize', resize)
})

onBeforeUnmount(() => {
  cancelAnimationFrame(animationFrame)
  window.removeEventListener('resize', resize)
})
</script>

<template>
  <canvas
    ref="canvasRef"
    class="fixed inset-0 pointer-events-none z-0"
  />
</template>
