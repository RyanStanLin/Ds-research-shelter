import csv
import re
from pathlib import Path
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

ROOT=Path(__file__).resolve().parents[1]
PAPER=ROOT/"paper"
text=(PAPER/"manuscript.md").read_text(encoding="utf-8")

def blocks(text):
    lines=text.splitlines();i=0
    while i<len(lines):
        line=lines[i].strip()
        if not line: i+=1;continue
        if line.startswith("|"):
            table=[]
            while i<len(lines) and lines[i].strip().startswith("|"):
                s=lines[i].strip()
                if not re.fullmatch(r"[| :\-]+",s):
                    table.append([c.strip() for c in s.strip("|").split("|")])
                i+=1
            yield("table",table);continue
        if line.startswith("!["):
            yield("image",re.search(r"\]\((.+)\)",line).group(1));i+=1;continue
        if line.startswith("#"):
            level=len(line)-len(line.lstrip("#"));yield("heading",(level,line.lstrip("# ")));i+=1;continue
        para=[line];i+=1
        while i<len(lines) and lines[i].strip() and not lines[i].strip().startswith(("#","|","![")):
            para.append(lines[i].strip());i+=1
        yield("para"," ".join(para))

def add_runs(p,value):

    for part in re.split(r"(\*[^*]+\*)",value):
        italic=part.startswith("*") and part.endswith("*") and len(part)>1
        chunk=part[1:-1] if italic else part
        for token in re.split(r"(g_z)",chunk):
            if token=="g_z":
                p.add_run("g").italic=True
                run=p.add_run("z");run.font.subscript=True
            else:
                run=p.add_run(token);run.italic=italic
            run.font.name="Times New Roman"

doc=Document()
section=doc.sections[0]
section.page_width=Inches(8.5);section.page_height=Inches(11)
section.top_margin=section.bottom_margin=section.left_margin=section.right_margin=Inches(1)
normal=doc.styles["Normal"];normal.font.name="Times New Roman";normal.font.size=Pt(12);normal.font.color.rgb=RGBColor(0,0,0)
normal.paragraph_format.line_spacing=1.5;normal.paragraph_format.space_after=Pt(6)
for name in ("Title","Subtitle","Heading 1","Heading 2","Heading 3","Caption"):
    style=doc.styles[name];style.font.name="Times New Roman";style.font.color.rgb=RGBColor(0,0,0)
    style.paragraph_format.space_before=Pt(10);style.paragraph_format.space_after=Pt(5)
    style.paragraph_format.keep_with_next=True
doc.styles["Title"].font.size=Pt(16);doc.styles["Title"].font.bold=True
doc.styles["Heading 1"].font.size=Pt(13);doc.styles["Heading 1"].font.bold=True
doc.styles["Heading 2"].font.size=Pt(12);doc.styles["Heading 2"].font.bold=True
doc.styles["Caption"].font.size=Pt(10.5)
for style in doc.styles:
    for border in list(style.element.iter(qn("w:pBdr"))):
        border.getparent().remove(border)
    for font in style.element.iter(qn("w:rFonts")):
        for name in ("asciiTheme","hAnsiTheme","eastAsiaTheme","cstheme"):
            font.attrib.pop(qn("w:"+name),None)
        for name in ("ascii","hAnsi","eastAsia","cs"):font.set(qn("w:"+name),"Times New Roman")
footer=section.footer.paragraphs[0];footer.alignment=WD_ALIGN_PARAGRAPH.RIGHT
field=OxmlElement("w:fldSimple");field.set(qn("w:instr"),"PAGE");footer._p.append(field)
refs=False
for kind,value in blocks(text):
    if kind=="heading":
        level,title=value
        if title=="References" or title.startswith("Appendix"):doc.add_page_break()
        refs=title=="References"
        p=doc.add_paragraph(style="Title" if level==1 else "Heading "+str(level-1))
        if level==1:p.alignment=WD_ALIGN_PARAGRAPH.CENTER
        add_runs(p,title)
    elif kind=="image":
        p=doc.add_paragraph();p.paragraph_format.keep_with_next=True
        p.alignment=WD_ALIGN_PARAGRAPH.CENTER
        p.add_run().add_picture(str((PAPER/value).resolve()),width=Inches(6.45))
    elif kind=="table":
        cols=len(value[0]);table=doc.add_table(rows=0,cols=cols)
        table.autofit=False
        widths={4:[2.35,1.05,1.15,1.95],5:[1.5,.42,1.88,2.18,.52],2:[3.75,2.75]}[cols]
        for c,w in zip(table.columns,widths):c.width=Inches(w)
        for ri,row in enumerate(value):
            cells=table.add_row().cells
            for ci,cell in enumerate(cells):
                cell.width=Inches(widths[ci]);p=cell.paragraphs[0]
                p.paragraph_format.line_spacing=1.0;p.paragraph_format.space_after=Pt(4);p.paragraph_format.space_before=Pt(4)
                if cols>2:p.paragraph_format.keep_with_next=True
                add_runs(p,row[ci])
                for run in p.runs:run.font.size=Pt(10.5);run.bold=(ri==0)
                tcpr=cell._tc.get_or_add_tcPr()
                borders=OxmlElement("w:tcBorders")
                for side in ("top","bottom","left","right"):
                    b=OxmlElement("w:"+side);b.set(qn("w:val"),"single");b.set(qn("w:sz"),"4");b.set(qn("w:color"),"C8C8C8");borders.append(b)
                tcpr.append(borders)
                if ri==0:
                    shading=OxmlElement("w:shd");shading.set(qn("w:fill"),"F0F0F0");tcpr.append(shading)
            trpr=table.rows[-1]._tr.get_or_add_trPr()
            trpr.append(OxmlElement("w:cantSplit"))
            if ri==0:trpr.append(OxmlElement("w:tblHeader"))
        gap=doc.add_paragraph();gap.paragraph_format.space_after=Pt(0)
        gap.paragraph_format.line_spacing=1;gap.paragraph_format.keep_with_next=(cols>2)
        gap.add_run().font.size=Pt(1)
    else:
        p=doc.add_paragraph()
        if refs:
            p.paragraph_format.left_indent=Inches(.4);p.paragraph_format.first_line_indent=Inches(-.4)
            p.paragraph_format.line_spacing=1.15
        elif value.startswith(("Figure ","Table ")):
            p.style="Caption"
            p.paragraph_format.keep_with_next=value.startswith("Table ")
        elif value.startswith("Note."):
            p.paragraph_format.line_spacing=1.0
            p.paragraph_format.space_after=Pt(10)
        add_runs(p,value)
doc.core_properties.title="Reading Proficiency and Accessible Presentation in DHH Classroom Related Comprehension"
doc.core_properties.subject="Evidence review and exploratory meta analysis"
doc.core_properties.author=""
doc.save(PAPER/"DHH_Comprehension_Research.docx")

def esc(value):
    value=value.replace("−","-").replace("–","--").replace("—","---").replace("’","'").replace("“","\u201c").replace("”","\u201d")
    mp={"\\":r"\textbackslash{}","&":r"\&","%":r"\%","$":r"\$","#":r"\#","_":r"\_","{":r"\{","}":r"\}","~":r"\textasciitilde{}","^":r"\textasciicircum{}","²":r"\textsuperscript{2}"}
    return "".join(mp.get(c,c) for c in value)
def inline(value):
    result=[]
    for part in re.split(r"(\*[^*]+\*)",value):
        italic=part.startswith("*") and part.endswith("*") and len(part)>1
        if italic:part=part[1:-1]
        chunks=re.split(r"(https?://\S+)",part)
        rendered="".join(r"\url{"+x+"}" if x.startswith("http") else esc(x) for x in chunks)
        rendered=rendered.replace(r"g\_z",r"$g_z$")
        result.append(r"\textit{"+rendered+"}" if italic else rendered)
    return "".join(result)
def read_result(name):
    with (ROOT/"results"/name).open(encoding="utf-8",newline="") as f:return list(csv.DictReader(f))
def distribution_tex():
    rows=read_result("candidate_factor_counts.csv")
    lines=[r"\begin{center}\setlength{\unitlength}{1pt}\begin{picture}(435,185)"]
    for i,r in enumerate(rows):
        y=173-i*13
        shade="black" if r["selection_status"].startswith("selected") else "gray"

        width=int(r["supplied_publications"])*16
        lines.append(r"\put(0,"+str(y)+r"){\small "+esc(r["factor_name"])+r"}")
        lines.append(r"\put(178,"+str(y)+r"){\color{"+shade+r"}\rule{"+str(width)+r"pt}{6pt}}")
        lines.append(r"\put("+str(182+width)+","+str(y)+r"){\small "+r["supplied_publications"]+r"}")
    lines.append(r"\put(178,5){\small Supplied reports with overlapping tags}")
    lines.append(r"\end{picture}\end{center}")
    return "\n".join(lines)
def forest_tex():
    rows={x["effect_id"]:x for x in read_result("effect_sizes.csv")}
    m=next(x for x in read_result("meta_analysis.csv") if x["synthesis_id"]=="reading_primary")
    lines=[r"\begin{center}\setlength{\unitlength}{1pt}\begin{picture}(435,125)"]
    def point(y,name,estimate,lo,hi):
        base=179;scale=190
        left=base+scale*float(lo);right=base+scale*float(hi);x=base+scale*float(estimate)
        lines.append(r"\put(0,"+str(y)+r"){\small "+esc(name)+r"}")
        lines.append(r"\put("+f"{left:.3f},{y+3}"+r"){\line(1,0){"+f"{right-left:.3f}"+r"}}")
        lines.append(r"\put("+f"{x-2:.3f},{y+1}"+r"){\rule{4pt}{4pt}}")
        lines.append(r"\put(373,"+str(y)+r"){\small "+f"{float(estimate):.2f}"+r"}")
    for y,id,name in zip((100,80,60),("R01","R02","R03"),("Stinson school 48*","Stinson college 48*","Tyler school 20")):
        r=rows[id];point(y,name,r["display_estimate"],r["ci_lower"],r["ci_upper"])
    point(38,"Pooled Wald",m["pooled_r"],m["ci_lower_r"],m["ci_upper_r"])
    point(20,"Pooled modified KH",m["pooled_r"],m["mKH_ci_lower_r"],m["mKH_ci_upper_r"])
    lines += [r"\put(179,6){\line(1,0){190}}",r"\put(178,-7){\small 0}",r"\put(273,-7){\small 0.5}",r"\put(367,-7){\small 1}",r"\end{picture}\end{center}"]
    return "\n".join(lines)
tex=[r"\documentclass[12pt,letterpaper]{article}",r"\usepackage[utf8]{inputenc}",r"\usepackage[T1]{fontenc}",r"\usepackage{lmodern}",r"\usepackage[margin=1in]{geometry}",r"\usepackage{setspace,longtable,array,xcolor,xurl,hyperref}",r"\hypersetup{hidelinks}",r"\setlength{\parindent}{0pt}",r"\setlength{\parskip}{6pt}",r"\setstretch{1.3}",r"\begin{document}"]
for kind,value in blocks(text):
    if kind=="heading":
        level,title=value
        if level==1:tex += [r"\begin{center}\Large\bfseries "+esc(title)+r"\end{center}"]
        else:
            if title=="References" or title.startswith("Appendix"):tex.append(r"\clearpage")
            tex.append((r"\section*{" if level==2 else r"\subsection*{")+esc(title)+"}")
    elif kind=="image":
        tex.append(distribution_tex() if "factor_distribution" in value else forest_tex())
    elif kind=="table":
        cols=len(value[0])
        widths={4:[.36,.16,.18,.23],5:[.23,.065,.28,.31,.055],2:[.56,.39]}[cols]
        fmt="@{}"+"".join("p{"+f"{w:.3f}"+r"\linewidth}" for w in widths)+"@{}"
        tex.append(r"{\small\setlength{\tabcolsep}{2pt}\renewcommand{\arraystretch}{1.15}\begin{longtable}{"+fmt+"}")
        tex.append(" & ".join(r"\textbf{"+inline(c)+"}" for c in value[0])+r"\\\hline\endhead")
        for row in value[1:]:tex.append(" & ".join(inline(c) for c in row)+r"\\")
        tex.append(r"\end{longtable}}")
    else:
        tex.append(inline(value)+r"\par")
tex.append(r"\end{document}")
(PAPER/"DHH_Comprehension_Research.tex").write_text("\n".join(tex),encoding="utf-8")
print("Created DOCX and standalone LaTeX from the same C#-exported manuscript.")
