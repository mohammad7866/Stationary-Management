import { useAuth } from "./AuthContext";

export function useRoleFlags() {
  const { roles = [] } = useAuth();
  const isUser  = roles.includes("User");
  const isAdmin = roles.includes("Admin");
  const isSuper = roles.includes("SuperAdmin");

  return {
    isUser, isAdmin, isSuper,
    canCreate:   isAdmin || isSuper,
    canEdit:     isAdmin || isSuper,
    canModerate: isAdmin || isSuper, // approve/reject/status changes
    canDelete:   isSuper,            // ONLY SuperAdmin
  };
}

export function IfRole({ anyOf = [], children }) {
  const { roles = [] } = useAuth();
  return anyOf.some(r => roles.includes(r)) ? children : null;
}
