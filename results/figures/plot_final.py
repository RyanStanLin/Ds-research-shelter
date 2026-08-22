import csv
from pathlib import Path
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import Polygon

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "results/figures"
def read(name):
    with (ROOT / "results" / name).open(encoding="utf-8", newline="") as f:
        return list(csv.DictReader(f))
def save(fig, name):
    for ext in ("png", "svg", "pdf"):
        fig.savefig(OUT / f"{name}.{ext}", dpi=250, bbox_inches="tight")
    plt.close(fig)
plt.rcParams.update({"font.family": "DejaVu Sans", "font.size": 12, "svg.fonttype": "none"})

rows = read("candidate_factor_counts.csv")
fig, ax = plt.subplots(figsize=(9.5, 5.5))
labels = [r["factor_name"] for r in rows]
bars = ax.barh(range(len(rows)), [int(r["supplied_publications"]) for r in rows],
               color=["#222222" if r["selection_status"].startswith("selected") else "#b8b8b8" for r in rows], height=.65)
for bar, r in zip(bars, rows):
    ax.text(bar.get_width()+.15, bar.get_y()+bar.get_height()/2, r["supplied_publications"], va="center")
ax.set_yticks(range(len(rows)), labels); ax.invert_yaxis()
ax.set_xlim(0, 12.5); ax.set_xticks(range(0, 13, 2))
ax.set_xlabel("Supplied full-text reports tagged to factor (overlapping counts)")
ax.set_title("DHH comprehension evidence distribution", loc="left", fontweight="bold", pad=15)
ax.grid(axis="x", color="#e4e4e4");ax.set_axisbelow(True)
for side in ("top","right","left"): ax.spines[side].set_visible(False)
ax.tick_params(axis="y", length=0)
fig.text(.03, -.025, "19 originals | Dark bars: selected factors. Tags do not imply a controlled manipulation.", fontsize=9)
fig.tight_layout()
save(fig, "factor_distribution")

effects = {r["effect_id"]:r for r in read("effect_sizes.csv")}
meta = next(r for r in read("meta_analysis.csv") if r["synthesis_id"]=="reading_primary")
weights = {r["effect_id"]:float(r["weight_percent"]) for r in read("synthesis_membership.csv") if r["synthesis_id"]=="reading_primary"}
names = ["Stinson 2009 school (n=48*)","Stinson 2009 college (n=48*)","Tyler 2009 school (n=20)"]
fig, ax = plt.subplots(figsize=(10,4.8))
ax.set_xlim(-.03,1.34); ax.set_ylim(-1.6,3.7)
for i,(id,label) in enumerate(zip(("R01","R02","R03"),names)):
    y=3-i; r=effects[id]; v=float(r["display_estimate"]); lo=float(r["ci_lower"]); hi=float(r["ci_upper"])
    ax.plot([lo,hi],[y,y],color="black",lw=1.3)
    ax.scatter(v,y,s=weights[id]*3,marker="s",color="#444444",zorder=4)
    ax.text(1.04,y,f"{v:.2f} [{lo:.2f}, {hi:.2f}]",va="center",fontsize=12)
pooled=float(meta["pooled_r"]);lo=float(meta["ci_lower_r"]);hi=float(meta["ci_upper_r"])
ax.add_patch(Polygon([[lo,0],[pooled,.17],[hi,0],[pooled,-.17]],closed=True,facecolor="black"))
ax.plot([float(meta["mKH_ci_lower_r"]),float(meta["mKH_ci_upper_r"])],[-.52,-.52],color="#555555",lw=2)
ax.plot([pooled,pooled],[-.67,-.37],color="black",lw=1.4)
ax.text(1.04,0,f"{pooled:.2f} [{lo:.2f}, {hi:.2f}]",va="center")
ax.text(1.04,-.52,f"[{float(meta['mKH_ci_lower_r']):.2f}, {float(meta['mKH_ci_upper_r']):.2f}]",va="center")
ax.set_yticks([3,2,1,0,-.52],names+["Pooled r — DL Wald","Pooled interval — modified KH"])
ax.set_xticks([0,.2,.4,.6,.8,1]);ax.set_xlabel("Pearson r (Fisher-z analysis, back-transformed)")
ax.axvline(0,color="#777777",ls="--",lw=.8)
ax.set_title("Reading proficiency and content comprehension",loc="left",fontweight="bold",pad=23)
ax.text(1.04,3.55,"r [95% CI]",fontsize=12,fontweight="bold")
for side in ("top","right","left"): ax.spines[side].set_visible(False)
ax.spines["bottom"].set_bounds(0,1);ax.tick_params(axis="y",length=0)
fig.text(.03,.005,"3 cohorts, 2 reports | *P05 pairwise n derived from complete cohorts. Unadjusted association; small-study uncertainty.",fontsize=9)
fig.subplots_adjust(left=.38,right=.96,top=.83,bottom=.21)
save(fig,"reading_forest")

fig, axes=plt.subplots(2,1,figsize=(10,6.5))
groups=[
    (axes[0],[("E01","Caption addition — Yoon 2011"),("E03","C-Print vs interpreting — Marschark 2006")],"Independent-group Hedges g"),
    (axes[1],[("E18","Caption editing — Ward 2007"),("E26","SimCom vs speech in quiet — Blom 2015"),("E28","SimCom vs speech in noise — Blom 2017"),("E29","Quiet vs noisy speech — Blom 2017")],"Paired difference-standardized g_z")]
for ax, pairs, title in groups:
    for i,(id,label) in enumerate(pairs):
        r=effects[id];v=float(r["display_estimate"]);lo=float(r["ci_lower"]);hi=float(r["ci_upper"])
        ax.errorbar(v,i,xerr=[[v-lo],[hi-v]],fmt="s",color="black",capsize=3)
        ax.text(1.7,i,f"{v:.2f} [{lo:.2f}, {hi:.2f}]",va="center",fontsize=9)
    ax.set_yticks(range(len(pairs)),[label for _,label in pairs]);ax.invert_yaxis()
    ax.set_xlim(-1.05,2.3);ax.set_xticks([-1,0,.5,1,1.5]);ax.axvline(0,color="#777",ls="--")
    ax.set_title(title,loc="left",fontweight="bold")
    ax.tick_params(axis="y",length=0)
    for side in ("top","right","left"):ax.spines[side].set_visible(False)
    ax.spines["bottom"].set_bounds(-1,1.5)
fig.subplots_adjust(left=.43,right=.98,top=.90,bottom=.13,hspace=.75)
fig.text(.04,.02,"Separate contrasts; no pooled teaching-format estimate. Different SD denominators. Blom 2017 contrasts share a cohort and passage confound.",fontsize=8.5)
save(fig,"presentation_contrasts")
