<template>
  <div :class="containerClass">
    <div class="w-[25%] md:w-[10%] flex justify-start">
      <div v-if="medalSrc" class="h-[30px]">
        <img 
          :src="medalSrc" 
          class="w-full h-full rounded-full object-contain"
        />
      </div>

      <span 
        v-else 
        class="w-[20px] h-[20px]"
      >
        {{ firstCol }}
      </span>
    </div>



    <div class="w-full flex space-x-2 items-center">
      <div
        v-if="!header"
        class="w-[35px] h-[35px] rounded-full"
      >
        <UAvatar :src="profileCol" />
      </div>
      <span>{{ secondCol }}</span>
    </div>

    <div class="w-[20%] md:w-[15%]">
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

const rankBgClass = computed(() =>
  props.header
    ? ''
    : ({
      1: 'bg-[#F3E1A3] dark:text-inverted',
        2: 'bg-[#E2E5EA] dark:text-inverted',
        3: 'bg-[#E8C2A8] dark:text-inverted'
      } as Record<number, string>)[props.position ?? 0] || ''
)

const containerClass = computed(() => [
  'flex items-center px-4 py-3 border border-[#ECECEC] text-[13px] sm:text-[15px] tracking-tight line-clamp-1',
  props.header
    ? 'bg-primary font-semibold rounded-t-xl'
    : ['font-medium', rankBgClass.value]
])

const medalSrc = computed(() => {
  if (props.header) return null

  const medals: Record<number, string> = {
    1: '/medals/gold.png',
    2: '/medals/silver.png',
    3: '/medals/bronze.png'
  }

  return medals[props.position ?? 0] || null
})
</script>

