<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref } from 'vue'

const canvasRef = ref<HTMLCanvasElement | null>(null)

let ctx!: CanvasRenderingContext2D
// eslint-disable-next-line @typescript-eslint/no-explicit-any
let stars: any[] = []
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const shootingStars: any[] = []
let animationFrame = 0
let width = 0
let height = 0
let starRGB = '255,255,255'
let observer: MutationObserver | null = null
let resizeTimeoutId: number | null = null

let time = 0
let lastTime = 0

const STAR_COUNT = 160
const STAR_DRIFT_SPEED = -8

function getStarColor() {
  const bg = getComputedStyle(document.documentElement)
    .getPropertyValue('--ui-bg')
    .trim()

  const match = bg.match(/(\d+)%\)$/)
  const lightness = match ? parseInt(match[1] || '50') : 50
  const isDark = lightness < 50

  if (isDark) {
    starRGB = '255, 251, 238'
  } else {
    starRGB = '35, 35, 34'
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

function resizeAndRebuild() {
  resize()
  createStars()
}

function handleResize() {
  // Resize canvas immediately for visual continuity
  resize()

  // Clear existing timeout
  if (resizeTimeoutId !== null) {
    clearTimeout(resizeTimeoutId)
  }

  // Debounce star recreation
  resizeTimeoutId = window.setTimeout(() => {
    createStars()
    resizeTimeoutId = null
  }, 500)
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
      offset: Math.random() * Math.PI * 2,
      vx: STAR_DRIFT_SPEED + Math.random() * 3
    })
  }
}

function createShootingStar() {
  let startX: number
  let startY: number

  // Randomly choose to start from top or left edge
  if (Math.random() < 0.5) {
    // Start from top edge
    startX = Math.random() * width
    startY = 0
  } else {
    // Start from left edge
    startX = 0
    startY = Math.random() * height * 0.5
  }

  shootingStars.push({
    x: startX,
    y: startY,
    vx: 6 + Math.random() * 2,
    vy: 1.5 + Math.random(),
    life: 0,
    maxLife: 100
  })
}

function updateStars(delta: number) {
  stars.forEach((star) => {
    star.x += star.vx * delta
    star.y += 1 * delta

    // Wrap around: when star goes off left, reappear on right
    if (star.x + star.radius < 0) {
      star.x = width + star.radius
      star.y = Math.random() * height
      star.offset = Math.random() * Math.PI * 2
    }
  })
}

function drawStars() {
  stars.forEach((star) => {
    const shimmer
      = star.baseOpacity
        + Math.sin(time * star.speed + star.offset)
        * star.amplitude

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

  updateStars(delta)
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
  resizeAndRebuild()
  animate(0)

  observer = new MutationObserver(() => {
    getStarColor()
  })

  observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['class']
  })

  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  cancelAnimationFrame(animationFrame)
  window.removeEventListener('resize', handleResize)
  if (resizeTimeoutId !== null) {
    clearTimeout(resizeTimeoutId)
  }
  observer?.disconnect()
})
</script>

<template>
  <canvas
    ref="canvasRef"
    class="fixed inset-0 pointer-events-none z-0"
  />
</template>
