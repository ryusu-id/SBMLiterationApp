<template>
  <section class="py-[40px] md:py-[120px]">
    <UContainer>
      <div class="grid grid-cols-12 gap-8">
        <!-- LEFT CONTENT -->
        <div class="col-span-12 lg:col-span-7 flex flex-col justify-between">
          <div class="font-poppins tracking-tight text-[60px] font-[500] mb-8">
            <h1 class="leading-tight">
              Discover
            </h1>
            <h1 class="leading-tight">
              The Super
            </h1>
            <h1 class="leading-tight">
              {{ "{" }}App Name{{ "}" }}
            </h1>
          </div>

          <p class="w-full lg:w-[80%] font-[300] text-justify">
            Lorem ipsum dolor sit amet consectetur adipisicing elit. Doloremque
            minus sint vero distinctio fuga aut consequuntur!
          </p>
        </div>

        <!-- RIGHT FEATURES -->
        <div class="col-span-12 lg:col-span-5 p-6 rounded-xl">
          <h1 class="text-[30px] font-[500] mb-8 leading-none tracking-tight">
            Powerful Features Reading Experiences
          </h1>

          <ul class="flex flex-col space-y-4">
            <li
              v-for="i in 3"
              :key="i"
              class="flex space-x-3"
            >
              <div class="p-2 rounded-full w-fit h-fit flex items-center">
                <UIcon
                  name="i-heroicons-trophy"
                  class="size-6"
                />
              </div>
              <div>
                <h2 class="text-[18px] font-[500]">
                  Leaderboard
                </h2>
                <p class="text-[13px] font-[300] tracking-tight">
                  Lorem ipsum dolor sit amet consectetur adipisicing elit.
                </p>
              </div>
            </li>
          </ul>
        </div>
      </div>
    </UContainer>
  </section>
</template>
