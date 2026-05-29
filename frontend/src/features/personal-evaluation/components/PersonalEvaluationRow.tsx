import { useEffect, useState } from "react";
import { ScoreInput } from "@/features/personal-evaluation/components/ScoreInput";
import type {
  PersonalEvaluationItem,
  SaveItemPayload,
} from "@/features/personal-evaluation/types";

interface Access {
  canText: boolean;
  canSelf: boolean;
  canTeam: boolean;
  canManager: boolean;
  canDeputy: boolean;
  canHead: boolean;
  canDelete: boolean;
}

interface Props {
  index: number;
  item: PersonalEvaluationItem;
  access: Access;
  onSave: (id: string, payload: SaveItemPayload) => void;
  onDelete: (id: string) => void;
}

function avg(values: Array<number | null>): number | null {
  const arr = values.filter((v): v is number => v != null && Number.isFinite(v));
  if (arr.length === 0) return null;
  return Math.round((arr.reduce((a, b) => a + b, 0) / arr.length) * 10) / 10;
}

function toIsoDate(v: string): string | null {
  if (!v) return null;
  return new Date(v + "T00:00:00").toISOString();
}

function fromIsoDate(v: string | null): string {
  if (!v) return "";
  return v.slice(0, 10);
}

export function PersonalEvaluationRow({ index, item, access, onSave, onDelete }: Props) {
  const [draft, setDraft] = useState<PersonalEvaluationItem>(item);

  useEffect(() => {
    setDraft(item);
  }, [item]);

  function commit(patch: Partial<PersonalEvaluationItem>) {
    const next = { ...draft, ...patch };
    setDraft(next);
    const { id, periodId, ...payload } = next;
    onSave(id, payload as SaveItemPayload);
  }

  function patchLocal(patch: Partial<PersonalEvaluationItem>) {
    setDraft((d) => ({ ...d, ...patch }));
  }

  const avgProgress = avg([
    draft.selfProgressScore,
    draft.teamLeadProgressScore,
    draft.managerProgressScore,
    draft.deputyProgressScore,
    draft.headProgressScore,
  ]);
  const avgQuality = avg([
    draft.selfQualityScore,
    draft.teamLeadQualityScore,
    draft.managerQualityScore,
    draft.deputyQualityScore,
    draft.headQualityScore,
  ]);

  const textRO = !access.canText;

  return (
    <tr>
      <td className="c-stt">{index + 1}</td>
      <td className="c-p-source">
        <textarea
          className="inp-sm"
          value={draft.assignmentSource ?? ""}
          readOnly={textRO}
          data-readonly={textRO ? "true" : undefined}
          onChange={(e) => patchLocal({ assignmentSource: e.target.value })}
          onBlur={() => commit({ assignmentSource: draft.assignmentSource })}
        />
      </td>
      <td className="c-p-task">
        <textarea
          className="inp-sm"
          value={draft.taskName ?? ""}
          readOnly={textRO}
          data-readonly={textRO ? "true" : undefined}
          onChange={(e) => patchLocal({ taskName: e.target.value })}
          onBlur={() => commit({ taskName: draft.taskName })}
        />
      </td>
      <td className="c-p-detail">
        <textarea
          className="inp-sm"
          value={draft.taskDetail ?? ""}
          readOnly={textRO}
          data-readonly={textRO ? "true" : undefined}
          onChange={(e) => patchLocal({ taskDetail: e.target.value })}
          onBlur={() => commit({ taskDetail: draft.taskDetail })}
        />
      </td>
      <td className="c-p-deadline">
        <input
          className="inp-sm"
          type="date"
          value={fromIsoDate(draft.deadline)}
          readOnly={textRO}
          disabled={textRO}
          onChange={(e) => commit({ deadline: toIsoDate(e.target.value) })}
        />
      </td>
      <td className="c-p-result">
        <textarea
          className="inp-sm"
          value={draft.actualResult ?? ""}
          readOnly={textRO}
          data-readonly={textRO ? "true" : undefined}
          onChange={(e) => patchLocal({ actualResult: e.target.value })}
          onBlur={() => commit({ actualResult: draft.actualResult })}
        />
      </td>
      <td className="c-p-completed">
        <input
          className="inp-sm"
          type="date"
          value={fromIsoDate(draft.completedAt)}
          readOnly={textRO}
          disabled={textRO}
          onChange={(e) => commit({ completedAt: toIsoDate(e.target.value) })}
        />
      </td>
      <td className="c-p-score"><ScoreInput value={draft.selfProgressScore} readOnly={!access.canSelf} onCommit={(v) => commit({ selfProgressScore: v })} /></td>
      <td className="c-p-score"><ScoreInput value={draft.selfQualityScore} readOnly={!access.canSelf} onCommit={(v) => commit({ selfQualityScore: v })} /></td>
      <td className="c-p-score"><ScoreInput value={draft.teamLeadProgressScore} readOnly={!access.canTeam} onCommit={(v) => commit({ teamLeadProgressScore: v })} /></td>
      <td className="c-p-score"><ScoreInput value={draft.teamLeadQualityScore} readOnly={!access.canTeam} onCommit={(v) => commit({ teamLeadQualityScore: v })} /></td>
      <td className="c-p-score"><ScoreInput value={draft.managerProgressScore} readOnly={!access.canManager} onCommit={(v) => commit({ managerProgressScore: v })} /></td>
      <td className="c-p-score"><ScoreInput value={draft.managerQualityScore} readOnly={!access.canManager} onCommit={(v) => commit({ managerQualityScore: v })} /></td>
      <td className="c-p-score"><ScoreInput value={draft.deputyProgressScore} readOnly={!access.canDeputy} onCommit={(v) => commit({ deputyProgressScore: v })} /></td>
      <td className="c-p-score"><ScoreInput value={draft.deputyQualityScore} readOnly={!access.canDeputy} onCommit={(v) => commit({ deputyQualityScore: v })} /></td>
      <td className="c-p-score"><ScoreInput value={draft.headProgressScore} readOnly={!access.canHead} onCommit={(v) => commit({ headProgressScore: v })} /></td>
      <td className="c-p-score"><ScoreInput value={draft.headQualityScore} readOnly={!access.canHead} onCommit={(v) => commit({ headQualityScore: v })} /></td>
      <td className="c-p-score">
        <input className="inp-sm avg-readonly" readOnly value={avgProgress ?? ""} />
      </td>
      <td className="c-p-score">
        <input className="inp-sm avg-readonly" readOnly value={avgQuality ?? ""} />
      </td>
      <td className="c-p-note">
        <textarea
          className="inp-sm"
          value={draft.note ?? ""}
          readOnly={textRO}
          data-readonly={textRO ? "true" : undefined}
          onChange={(e) => patchLocal({ note: e.target.value })}
          onBlur={() => commit({ note: draft.note })}
        />
      </td>
      <td className="c-act">
        <button
          type="button"
          className="personal-del"
          disabled={!access.canDelete}
          onClick={() => onDelete(item.id)}
          title="Xóa dòng"
        >
          <svg width="11" height="11" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
            <polyline points="3 6 5 6 21 6" />
            <path d="M19 6l-1 14H6L5 6" />
          </svg>
        </button>
      </td>
    </tr>
  );
}
