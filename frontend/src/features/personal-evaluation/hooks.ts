import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { personalEvaluationApi } from "@/features/personal-evaluation/api";
import type {
  PersonalEvaluationItem,
  PersonalEvaluationPeriod,
  PersonalEvaluationResponse,
  SaveItemPayload,
  SavePeriodPayload,
} from "@/features/personal-evaluation/types";

export const personalEvaluationKeys = {
  detail: (year: number, month: number, userId: string | null) =>
    ["personal-evaluation", year, month, userId] as const,
  scorableUsers: ["personal-evaluation", "scorable-users"] as const,
};

export function usePersonalEvaluation(year: number, month: number, userId: string | null) {
  return useQuery({
    queryKey: personalEvaluationKeys.detail(year, month, userId),
    queryFn: () => personalEvaluationApi.get(year, month, userId),
    enabled: Boolean(userId),
    staleTime: 30 * 1000,
  });
}

export function useScorableUsers(enabled: boolean) {
  return useQuery({
    queryKey: personalEvaluationKeys.scorableUsers,
    queryFn: personalEvaluationApi.listScorableUsers,
    enabled,
    staleTime: 30 * 1000,
  });
}

export function useCreatePersonalItem(year: number, month: number, userId: string | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (periodId: string) => personalEvaluationApi.createItem(periodId),
    onSuccess: () => qc.invalidateQueries({ queryKey: personalEvaluationKeys.detail(year, month, userId) }),
  });
}

export function useSavePersonalItem(year: number, month: number, userId: string | null) {
  const qc = useQueryClient();
  const key = personalEvaluationKeys.detail(year, month, userId);
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: SaveItemPayload }) =>
      personalEvaluationApi.saveItem(id, payload),
    onMutate: async ({ id, payload }) => {
      await qc.cancelQueries({ queryKey: key });
      const prev = qc.getQueryData<PersonalEvaluationResponse>(key);
      if (prev) {
        qc.setQueryData<PersonalEvaluationResponse>(key, {
          ...prev,
          items: prev.items.map((it) => (it.id === id ? { ...it, ...payload } as PersonalEvaluationItem : it)),
        });
      }
      return { prev };
    },
    onError: (_e, _v, ctx) => {
      if (ctx?.prev) qc.setQueryData(key, ctx.prev);
    },
    onSettled: () => qc.invalidateQueries({ queryKey: key }),
  });
}

export function useDeletePersonalItem(year: number, month: number, userId: string | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => personalEvaluationApi.deleteItem(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: personalEvaluationKeys.detail(year, month, userId) }),
  });
}

export function useSavePersonalPeriod(year: number, month: number, userId: string | null) {
  const qc = useQueryClient();
  const key = personalEvaluationKeys.detail(year, month, userId);
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: SavePeriodPayload }) =>
      personalEvaluationApi.savePeriod(id, payload),
    onMutate: async ({ payload }) => {
      await qc.cancelQueries({ queryKey: key });
      const prev = qc.getQueryData<PersonalEvaluationResponse>(key);
      if (prev) {
        qc.setQueryData<PersonalEvaluationResponse>(key, {
          ...prev,
          period: { ...prev.period, ...payload } as PersonalEvaluationPeriod,
        });
      }
      return { prev };
    },
    onError: (_e, _v, ctx) => {
      if (ctx?.prev) qc.setQueryData(key, ctx.prev);
    },
    onSettled: () => qc.invalidateQueries({ queryKey: key }),
  });
}
