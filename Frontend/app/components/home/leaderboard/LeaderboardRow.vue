<template>
  <div :class="containerClass">
    <div class="w-[25%] md:w-[10%]" :class="rankColorClass">
      <span class="w-[20px] h-[20px]">{{ firstCol }}</span>
    </div>

    <div class="w-full flex space-x-2 items-center" :class="rankColorClass">
      <div v-if="!header" class="w-[35px] h-[35px] rounded-full">
        <img :src="profileCol" class="w-full h-full" alt="">
      </div>
      <span>
        
        {{ secondCol }}
      </span>
    </div>

    <div class="w-[20%] md:w-[15%]" :class="rankColorClass">
      <span>{{ thirdCol }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  position?: number
  firstCol: string
  profileCol?: string
  secondCol: string
  thirdCol: string
  header?: boolean
}>()

const rankColorClass = computed(() =>
  props.header
    ? ''
    : ({
        1: 'text-[#E6B84D]',
        2: 'text-[#A8ADB3]',
        3: 'text-[#B26A3D]',
      } as Record<number, string>)[props.position ?? 0] || ''
)

const containerClass = computed(() => [
  'flex items-center px-4 py-3 border border-[#ECECEC] text-[13px] sm:text-[15px] tracking-tight line-clamp-1',
  props.header ? 'bg-primary font-semibold rounded-t-xl' : 'font-medium'
])
</script>
