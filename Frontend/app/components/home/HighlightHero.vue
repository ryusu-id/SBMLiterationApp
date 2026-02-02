<template>
  <section class="px-4 py-16">
    <UContainer>
      <div class="grid grid-cols-12 gap-y-8 lg:gap-y-0">
        <!-- LEFT -->
        <div class="col-span-12 lg:col-span-4 pt-8 border-transparent lg:pt-0 border-t lg:border-t-0 dark:border-gray-500  p-4 flex flex-col justify-between">
          <div class="font-poppins tracking-tight text-[37px] font-[400] mb-5">
            <h1 class="leading-tight">
              Join Our Community
            </h1>
            <h1 class="leading-tight">
              And Start Your
            </h1>
            <h1 class="leading-tight">
              Journey
            </h1>
          </div>

          <h1 class="underline">
            LET'S GO
          </h1>
        </div>

        <!-- MIDDLE -->
        <div class="col-span-12 lg:col-span-4 pt-8 lg:pt-0  border-t lg:border-l lg:border-t-0 border-gray-500 p-4 flex flex-col justify-between">
          <h1 class="text-[40px]">
            900+
          </h1>

          <p class="tracking-tight text-[13px] font-[300]">
            Lorem ipsum dolor sit amet consectetur, adipisicing elit. Nam amet nobis
            provident consequuntur ut eveniet enim eius fugit culpa.
          </p>
        </div>

        <!-- RIGHT -->
        <div class="col-span-12 lg:col-span-4 p-8 flex flex-col justify-between">
          <!-- Rating -->
          <div class="flex items-start justify-between ">
            <div class="flex items-start space-x-2 flex-1 mb-8 ">
              <h1 class="text-[60px] leading-none">
                4.9
              </h1>

              <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 24 24"
                fill="#fbd82b"
                class="size-6"
              >
                <path
                  fill-rule="evenodd"
                  d="M10.788 3.21c.448-1.077 1.976-1.077 2.424 0l2.082 5.006 5.404.434c1.164.093 1.636 1.545.749 2.305l-4.117 3.527 1.257 5.273c.271 1.136-.964 2.033-1.96 1.425L12 18.354 7.373 21.18c-.996.608-2.231-.29-1.96-1.425l1.257-5.273-4.117-3.527c-.887-.76-.415-2.212.749-2.305l5.404-.434 2.082-5.005Z"
                  clip-rule="evenodd"
                />
              </svg>
            </div>

            <h1 class="text-[20px] font-[200] text-right">
              in All The Platform Combined
            </h1>
          </div>

          <!-- Avatars -->
          <div class="flex items-center mb-8">
            <img
              v-for="i in 4"
              :key="i"
              :src="`https://i.pravatar.cc/100?img=${i}`"
              class="w-12 h-12 rounded-full border-2 border-base object-cover -ml-4 first:ml-0"
            >

            <div
              class="w-12 h-12 rounded-full bg-lime-400 flex items-center justify-center text-[10px] leading-[10px] border-2 border-base -ml-4"
            >
              100+<br>More
            </div>
          </div>

          <h1 class="text-[25px] tracking-tight">
            500+ <span class="text-[15px]">User Active</span>
          </h1>
        </div>
      </div>
    </UContainer>
  </section>
</template>
