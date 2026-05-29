import { toApiError } from "@/shared/api/client";
import { useAdminRoles } from "@/features/admin/hooks";
import "@/features/admin/AdminPages.css";

export function AdminRolesPage() {
  const rolesQuery = useAdminRoles();

  return (
    <div className="admin-page">
      <div className="admin-page__header">
        <h1>Danh muc vai tro</h1>
        <p className="admin-page__subtitle">Danh sach role he thong — chi doc, khong chinh sua.</p>
      </div>

      {rolesQuery.isLoading && <p className="admin-state">Dang tai...</p>}
      {rolesQuery.isError && (
        <p className="admin-state admin-state--error">{toApiError(rolesQuery.error).message}</p>
      )}

      {rolesQuery.data && (
        <div className="admin-table-wrapper">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Ma vai tro</th>
                <th>Ten vai tro</th>
              </tr>
            </thead>
            <tbody>
              {rolesQuery.data.map((role) => (
                <tr key={role.id}>
                  <td className="admin-table__mono">{role.code}</td>
                  <td>{role.name}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
