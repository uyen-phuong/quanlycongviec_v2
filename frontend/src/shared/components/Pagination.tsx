import { cn } from "@/shared/utils";
import { ChevronLeft, ChevronRight, MoreHorizontal } from "lucide-react";
import "./Pagination.css";

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  className?: string;
}

export function Pagination({
  currentPage,
  totalPages,
  onPageChange,
  className,
}: PaginationProps) {
  if (totalPages <= 1) return null;

  const getPageNumbers = () => {
    const pages: (number | "ellipsis")[] = [];
    if (totalPages <= 7) {
      for (let i = 1; i <= totalPages; i++) pages.push(i);
    } else {
      if (currentPage <= 3) {
        pages.push(1, 2, 3, 4, "ellipsis", totalPages);
      } else if (currentPage >= totalPages - 2) {
        pages.push(
          1,
          "ellipsis",
          totalPages - 3,
          totalPages - 2,
          totalPages - 1,
          totalPages
        );
      } else {
        pages.push(
          1,
          "ellipsis",
          currentPage - 1,
          currentPage,
          currentPage + 1,
          "ellipsis",
          totalPages
        );
      }
    }
    return pages;
  };

  return (
    <nav className={cn("pagination", className)}>
      <button
        type="button"
        className="pagination-btn"
        disabled={currentPage <= 1}
        onClick={() => onPageChange(currentPage - 1)}
        title="Trang trước"
      >
        <ChevronLeft size={16} />
      </button>

      {getPageNumbers().map((p, i) => {
        if (p === "ellipsis") {
          return (
            <span key={`ellipsis-${i}`} className="pagination-ellipsis">
              <MoreHorizontal size={16} />
            </span>
          );
        }
        return (
          <button
            key={p}
            type="button"
            className={cn("pagination-btn", {
              "pagination-btn--active": currentPage === p,
            })}
            onClick={() => onPageChange(p)}
          >
            {p}
          </button>
        );
      })}

      <button
        type="button"
        className="pagination-btn"
        disabled={currentPage >= totalPages}
        onClick={() => onPageChange(currentPage + 1)}
        title="Trang sau"
      >
        <ChevronRight size={16} />
      </button>
    </nav>
  );
}
