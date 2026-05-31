import { type ReactNode } from "react";
import { cn } from "@/shared/utils";
import "./DataTable.css";

export interface ColumnDef<T> {
  key: string;
  header: ReactNode;
  cell?: (item: T) => ReactNode;
  className?: string;
  width?: string | number;
}

interface DataTableProps<T> {
  data: T[];
  columns: ColumnDef<T>[];
  keyExtractor: (item: T) => string;
  isLoading?: boolean;
  emptyMessage?: string;
  className?: string;
  rowClassName?: (item: T) => string;
  renderRow?: (item: T) => ReactNode;
}

export function DataTable<T>({
  data,
  columns,
  keyExtractor,
  isLoading = false,
  emptyMessage = "Không có dữ liệu",
  className,
  rowClassName,
  renderRow,
}: DataTableProps<T>) {
  if (isLoading) {
    return (
      <div className={cn("data-table-wrapper", className)}>
        <div className="data-table-loading">
          <div className="data-table-skeleton" style={{ opacity: 0.7 }} />
          <div className="data-table-skeleton" style={{ opacity: 0.5 }} />
          <div className="data-table-skeleton" style={{ opacity: 0.3 }} />
        </div>
      </div>
    );
  }

  return (
    <div className={cn("data-table-wrapper", className)}>
      <table className="data-table">
        <thead>
          <tr>
            {columns.map((col) => (
              <th
                key={col.key}
                className={col.className}
                style={{ width: col.width }}
              >
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.length === 0 ? (
            <tr>
              <td colSpan={columns.length} className="data-table-empty">
                {emptyMessage}
              </td>
            </tr>
          ) : (
            data.map((item) =>
              renderRow ? (
                renderRow(item)
              ) : (
                <tr
                  key={keyExtractor(item)}
                  className={rowClassName ? rowClassName(item) : undefined}
                >
                  {columns.map((col) => (
                    <td key={col.key} className={col.className}>
                      {col.cell ? col.cell(item) : (item as any)[col.key]}
                    </td>
                  ))}
                </tr>
              )
            )
          )}
        </tbody>
      </table>
    </div>
  );
}
