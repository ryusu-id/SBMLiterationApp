import { handleResponseError } from './api'
import { $authedFetch } from '~/apis/api'

export interface PagingQuery {
  page?: number
  rowsPerPage?: number
  sortBy?: string
  sortDirection?: string
  search?: string
}

export interface PagingResult<T> {
  page: number
  rowsPerPage: number
  totalPages: number
  totalRows: number
  sortBy: string
  sortDirection: string
  searchText: string
  rows: T[]
}

export function usePaging<T>(apiRoute: string) {
  // loading concern
  const loading = ref(false)

  // paging param
  const page = ref(1)
  const rowsPerPage = ref(10)
  const searchText = ref('')

  // paging data
  const rows = ref<T[]>([])
  const totalRows = ref(0)
  const totalPages = ref(0)

  // fetch data based on paging param
  async function fetch() {
    loading.value = true
    try {
      const response = await $authedFetch<PagingResult<T>>(apiRoute, {
        query: {
          page: page.value,
          rowsPerPage: rowsPerPage.value,
          search: searchText.value
        }
      })

      if (response.rows && Array.isArray(response.rows)) {
        rows.value = response.rows
        totalRows.value = response.totalRows
        totalPages.value = response.totalPages
      }
    } catch (error) {
      handleResponseError(error)
    } finally {
      loading.value = false
    }
  }

  // search data based on search text
  async function search(searchTextParam: string) {
    searchText.value = searchTextParam
    page.value = 1
    await fetch()
  }

  // change page
  async function goTo(pageParam: number) {
    page.value = pageParam
    await fetch()
  }

  // change rows per page
  async function changeRowsPerPage(rowsPerPageParam: number) {
    rowsPerPage.value = rowsPerPageParam
    await fetch()
  }

  return {
    page: readonly(page),
    rowsPerPage: readonly(rowsPerPage),
    loading: readonly(loading),
    rows,
    totalRows: readonly(totalRows),
    totalPages: readonly(totalPages),
    fetch,
    search,
    goTo,
    changeRowsPerPage
  }
}
