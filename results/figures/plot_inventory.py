import csv
from pathlib import Path
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

ROOT = Path(__file__).resolve().parents[2]
with (ROOT / "results/candidate_factor_counts.csv").open(encoding="utf-8", newline="") as f:
    rows = list(csv.DictReader(f))

labels = [r["factor_name"] for r in rows]
labels[5] += " (caption-rate leads)"
labels[9] += " (indirect leads)"
series = [
    ("Core candidate reports", "core_candidate_reports", "#1f6b8c"),
    ("Near-scope / title-only reports", "near_scope_or_title_only_reports", "#8bc3cd"),
    ("Qualitative / review context", "context_reports", "#d3d8dc"),
]
plt.rcParams.update({"font.family": "DejaVu Sans", "font.size": 10, "svg.fonttype": "none"})
fig, ax = plt.subplots(figsize=(11, 7.5))
left = [0] * len(rows)
for label, key, color in series:
    values = [int(r[key]) for r in rows]
    ax.barh(range(len(rows)), values, left=left, label=label, color=color, height=.64)
    left = [a + b for a, b in zip(left, values)]
for i, total in enumerate(left):
    ax.text(total + .15, i, str(total), va="center", fontsize=10)
ax.set_yticks(range(len(rows)), labels)
ax.invert_yaxis()
ax.set_xlim(0, max(left) + 1.3)
ax.set_xticks(range(0, max(left) + 1, 3))
ax.set_xlabel("Candidate publication links (overlap across factors)")
ax.set_title("DHH comprehension research: preliminary evidence inventory", loc="left", weight="bold", pad=18)
ax.grid(axis="x", color="#e8ebee", linewidth=.7)
ax.set_axisbelow(True)
for side in ["top", "right", "left"]:
    ax.spines[side].set_visible(False)
ax.tick_params(axis="y", length=0)
ax.legend(loc="upper left", bbox_to_anchor=(0, -.10), frameon=False, ncol=1, fontsize=9)
fig.text(.02, .025,
    "2026-10-14 | Abstract/index screening only; no original PDFs verified.\n"
    "Report counts are not independent-study counts or evidence of an effect. Two population exclusions omitted.",
    ha="left", va="bottom", fontsize=9, color="#58616a")
fig.subplots_adjust(left=.34, right=.97, top=.89, bottom=.25)
for extension in ["png", "svg", "pdf"]:
    fig.savefig(ROOT / f"results/figures/preliminary_factor_distribution.{extension}", dpi=200)
plt.close(fig)
