import ConfirmationDialog from '~/components/ConfirmationDialog.vue'

interface ConfirmOptions {
  title: string
  subTitle: string
  message: string
  onOk?: () => void | Promise<void>
  onCancel?: () => void | Promise<void>
}

export function useDialog() {
  const confirm = async (options: ConfirmOptions) => {
    return new Promise<boolean>((resolve) => {
      const overlay = useOverlay().create(ConfirmationDialog)
      overlay.open({
        onOk: async () => {
          if (options.onOk)
            await options.onOk()
          overlay.close()
          resolve(true)
        },
        onCancel: async () => {
          if (options.onCancel)
            await options.onCancel()
          overlay.close()
          resolve(false)
        },
        title: options.title,
        subTitle: options.subTitle,
        message: options.message
      })
    })
  }

  return {
    confirm
  }
}
