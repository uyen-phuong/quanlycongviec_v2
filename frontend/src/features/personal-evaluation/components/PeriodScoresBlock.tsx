import { ScoreInput } from "@/features/personal-evaluation/components/ScoreInput";
import type { PersonalEvaluationPeriod, SavePeriodPayload } from "@/features/personal-evaluation/types";

interface ColAccess {
  self: boolean;
  teamLead: boolean;
  manager: boolean;
  deputy: boolean;
  head: boolean;
}

interface Props {
  period: PersonalEvaluationPeriod;
  access: ColAccess;
  onSave: (payload: SavePeriodPayload) => void;
}

function avg(values: Array<number | null>): number | null {
  const arr = values.filter((v): v is number => v != null && Number.isFinite(v));
  if (arr.length === 0) return null;
  return Math.round((arr.reduce((a, b) => a + b, 0) / arr.length) * 10) / 10;
}

export function PeriodScoresBlock({ period, access, onSave }: Props) {
  function buildPayload(): SavePeriodPayload {
    return {
      capacityAttitudeSelfScore: period.capacityAttitudeSelfScore,
      capacityAttitudeTeamLeadScore: period.capacityAttitudeTeamLeadScore,
      capacityAttitudeManagerScore: period.capacityAttitudeManagerScore,
      capacityAttitudeDeputyScore: period.capacityAttitudeDeputyScore,
      capacityAttitudeHeadScore: period.capacityAttitudeHeadScore,
      disciplineSelfScore: period.disciplineSelfScore,
      disciplineTeamLeadScore: period.disciplineTeamLeadScore,
      disciplineManagerScore: period.disciplineManagerScore,
      disciplineDeputyScore: period.disciplineDeputyScore,
      disciplineHeadScore: period.disciplineHeadScore,
      inspectionSelfScore: period.inspectionSelfScore,
      inspectionTeamLeadScore: period.inspectionTeamLeadScore,
      inspectionManagerScore: period.inspectionManagerScore,
      inspectionDeputyScore: period.inspectionDeputyScore,
      inspectionHeadScore: period.inspectionHeadScore,
    };
  }

  function commit(patch: Partial<SavePeriodPayload>) {
    onSave({ ...buildPayload(), ...patch });
  }

  function renderRow(
    label: string,
    max: number,
    keys: {
      self: keyof SavePeriodPayload;
      teamLead: keyof SavePeriodPayload;
      manager: keyof SavePeriodPayload;
      deputy: keyof SavePeriodPayload;
      head: keyof SavePeriodPayload;
    },
  ) {
    const valSelf = period[keys.self] as number | null;
    const valTeam = period[keys.teamLead] as number | null;
    const valManager = period[keys.manager] as number | null;
    const valDeputy = period[keys.deputy] as number | null;
    const valHead = period[keys.head] as number | null;
    const avgValue = avg([valSelf, valTeam, valManager, valDeputy, valHead]);
    return (
      <>
        <div className="label">{label}</div>
        <ScoreInput value={valSelf} max={max} readOnly={!access.self} onCommit={(v) => commit({ [keys.self]: v } as Partial<SavePeriodPayload>)} />
        <ScoreInput value={valTeam} max={max} readOnly={!access.teamLead} onCommit={(v) => commit({ [keys.teamLead]: v } as Partial<SavePeriodPayload>)} />
        <ScoreInput value={valManager} max={max} readOnly={!access.manager} onCommit={(v) => commit({ [keys.manager]: v } as Partial<SavePeriodPayload>)} />
        <ScoreInput value={valDeputy} max={max} readOnly={!access.deputy} onCommit={(v) => commit({ [keys.deputy]: v } as Partial<SavePeriodPayload>)} />
        <ScoreInput value={valHead} max={max} readOnly={!access.head} onCommit={(v) => commit({ [keys.head]: v } as Partial<SavePeriodPayload>)} />
        <div className="avg-cell">{avgValue ?? "—"}</div>
      </>
    );
  }

  return (
    <div className="period-scores-block">
      <h4>Điểm tổng kỳ (chấm 1 lần / kỳ báo cáo)</h4>
      <div className="period-scores-grid">
        <div className="label"></div>
        <div className="head">NLĐ tự</div>
        <div className="head">Tổ trưởng</div>
        <div className="head">Lãnh đạo phòng</div>
        <div className="head">Phó KTNB</div>
        <div className="head">Trưởng KTNB</div>
        <div className="head">Bình quân</div>

        {renderRow("Năng lực, thái độ (≤20)", 20, {
          self: "capacityAttitudeSelfScore",
          teamLead: "capacityAttitudeTeamLeadScore",
          manager: "capacityAttitudeManagerScore",
          deputy: "capacityAttitudeDeputyScore",
          head: "capacityAttitudeHeadScore",
        })}
        {renderRow("Ý thức kỷ luật, nội quy (≤10)", 10, {
          self: "disciplineSelfScore",
          teamLead: "disciplineTeamLeadScore",
          manager: "disciplineManagerScore",
          deputy: "disciplineDeputyScore",
          head: "disciplineHeadScore",
        })}
        {renderRow("Kiểm tra nghiệp vụ (≤10)", 10, {
          self: "inspectionSelfScore",
          teamLead: "inspectionTeamLeadScore",
          manager: "inspectionManagerScore",
          deputy: "inspectionDeputyScore",
          head: "inspectionHeadScore",
        })}
      </div>
    </div>
  );
}
