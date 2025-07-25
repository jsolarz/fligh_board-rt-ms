// Pagination component - Cyberpunk styled pagination controls
import React from "react"

interface PaginationProps {
  currentPage: number
  totalPages: number
  hasNext: boolean
  hasPrevious: boolean
  totalCount: number
  currentPageSize: number
  onPageChange: (page: number) => void
  isLoading?: boolean
}

const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  hasNext,
  hasPrevious,
  totalCount,
  currentPageSize,
  onPageChange,
  isLoading = false,
}) => {
  if (totalPages <= 1) {
    return null // Don't show pagination if only one page
  }

  const generatePageNumbers = () => {
    const pages: (number | string)[] = []
    const maxVisiblePages = 5

    if (totalPages <= maxVisiblePages) {
      // Show all pages if total is small
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i)
      }
    } else {
      // Complex pagination logic for large page counts
      pages.push(1)

      if (currentPage > 3) {
        pages.push("...")
      }

      const start = Math.max(2, currentPage - 1)
      const end = Math.min(totalPages - 1, currentPage + 1)

      for (let i = start; i <= end; i++) {
        if (i !== 1 && i !== totalPages) {
          pages.push(i)
        }
      }

      if (currentPage < totalPages - 2) {
        pages.push("...")
      }

      if (totalPages > 1) {
        pages.push(totalPages)
      }
    }

    return pages
  }

  const pageNumbers = generatePageNumbers()
  const startRecord = (currentPage - 1) * currentPageSize + 1
  const endRecord = Math.min(currentPage * currentPageSize, totalCount)

  return (
    <div className="holographic rounded-lg border border-neon-cyan/30 p-4 mt-6">
      <div className="flex flex-col md:flex-row items-center justify-between space-y-4 md:space-y-0">
        {/* Results Info */}
        <div className="font-cyber text-sm text-neon-cyan/80">
          <span className="text-neon-green">[DATA_RANGE]</span> {startRecord}-
          {endRecord} OF{" "}
          <span className="text-neon-cyan font-bold">{totalCount}</span>{" "}
          FLIGHTS_FOUND
        </div>

        {/* Pagination Controls */}
        <div className="flex items-center space-x-2">
          {/* Previous Button */}
          <button
            onClick={() => onPageChange(currentPage - 1)}
            disabled={!hasPrevious || isLoading}
            className={`cyber-button-sm border-neon-cyan text-neon-cyan ${
              !hasPrevious || isLoading
                ? "opacity-50 cursor-not-allowed"
                : "hover:shadow-neon-md"
            }`}
          >
            ← PREV
          </button>

          {/* Page Numbers */}
          <div className="flex items-center space-x-1">
            {pageNumbers.map((page, index) => (
              <React.Fragment key={index}>
                {page === "..." ? (
                  <span className="px-2 py-1 text-neon-cyan/60 font-cyber text-sm">
                    ...
                  </span>
                ) : (
                  <button
                    onClick={() => onPageChange(page as number)}
                    disabled={isLoading}
                    className={`px-3 py-1 font-cyber text-sm border rounded transition-all duration-300 ${
                      currentPage === page
                        ? "bg-neon-cyan/20 border-neon-cyan text-neon-cyan shadow-neon-sm"
                        : "border-neon-cyan/30 text-neon-cyan/70 hover:border-neon-cyan hover:text-neon-cyan hover:shadow-neon-sm"
                    } ${
                      isLoading
                        ? "opacity-50 cursor-not-allowed"
                        : "hover:scale-105"
                    }`}
                  >
                    {page}
                  </button>
                )}
              </React.Fragment>
            ))}
          </div>

          {/* Next Button */}
          <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={!hasNext || isLoading}
            className={`cyber-button-sm border-neon-cyan text-neon-cyan ${
              !hasNext || isLoading
                ? "opacity-50 cursor-not-allowed"
                : "hover:shadow-neon-md"
            }`}
          >
            NEXT →
          </button>
        </div>

        {/* Page Size Info */}
        <div className="font-cyber text-xs text-neon-cyan/60">
          PAGE {currentPage} OF {totalPages}
        </div>
      </div>

      {/* Loading Indicator */}
      {isLoading && (
        <div className="flex items-center justify-center mt-3 pt-3 border-t border-neon-cyan/20">
          <div className="flex items-center space-x-2 text-neon-cyan">
            <div className="animate-spin rounded-full h-4 w-4 border-2 border-neon-cyan border-t-transparent"></div>
            <span className="text-sm font-cyber">LOADING_DATA_STREAM...</span>
          </div>
        </div>
      )}
    </div>
  )
}

export default Pagination
