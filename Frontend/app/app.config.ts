export default defineAppConfig({
  ui: {
    colors: {
      primary: 'green',
      neutral: 'slate'
    },
    table: {
      slots: {
        root: 'border border-neutral-200 dark:border-neutral-700 rounded-lg',
        td: 'p-2',
        th: 'p-2'
      }
    }
  }
})
