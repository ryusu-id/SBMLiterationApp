<script setup lang='ts'>
import { ref, onMounted, onBeforeUnmount } from 'vue'

const fullText = [
  'Cuma perlu satu buku untuk jatuh cinta pada membaca.',
  'Cari buku itu, mari jatuh cinta.'
]

const displayedLines = ref<string[]>(['', ''])

let lineIndex = 0
let charIndex = 0
let typingInterval: ReturnType<typeof setInterval> | null = null
let loopTimeout: ReturnType<typeof setTimeout> | null = null

const startTyping = (): void => {
  displayedLines.value = ['', '']
  lineIndex = 0
  charIndex = 0

  typingInterval = setInterval(() => {
    const currentLine = fullText[lineIndex]

    if (charIndex < currentLine.length) {
      displayedLines.value[lineIndex] += currentLine[charIndex]
      charIndex++
    } else {
      if (lineIndex < fullText.length - 1) {
        lineIndex++
        charIndex = 0
      } else {
        if (typingInterval) clearInterval(typingInterval)

        loopTimeout = setTimeout(() => {
          startTyping()
        }, 5000)
      }
    }
  }, 50)
}

onMounted(() => {
  startTyping()
})

onBeforeUnmount(() => {
  if (typingInterval) clearInterval(typingInterval)
  if (loopTimeout) clearTimeout(loopTimeout)
})
</script>


<template>
  <section class="py-[40px] md:py-[60px]">
    <UContainer>
      <div class="flex justify-center text-3xl mb-5">
        <Icon name='ph:quotes-fill' />
      </div>
     <div class="h-[190px] sm:h-[140px] lg:h-[108px] xl:h-[102px]">
      <h1 class='typewriter font-semibold text-3xl lg:text-3xl xl:text-4xl tracking-tight text-center dark:text-primary'>
        <span v-for='(line, i) in displayedLines' :key='i'>
          {{ line }}<br v-if='i < displayedLines.length - 1' />
        </span>
      </h1>
     </div>
     <div class="flex justify-center">
      <h1 class="tracking-tight font-semibold italic">- Najwa Shihab -</h1>
     </div>
    </UContainer>
  </section>
</template>

