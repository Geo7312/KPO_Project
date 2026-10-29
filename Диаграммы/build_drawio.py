"""Build editable, uncompressed diagrams.net UML diagrams for RepairDesk."""

from pathlib import Path
import xml.etree.ElementTree as ET


HERE = Path(__file__).resolve().parent
OUTPUT = HERE / "RepairDesk_UML.drawio"

INK = "#17324F"
BLUE = "#2A7296"
PALE = "#EAF6FC"
PANEL = "#F4FAFD"
ACCENT = "#BCE5F6"


class Page:
    def __init__(self, parent, page_id, name, width, height):
        self.diagram = ET.SubElement(parent, "diagram", {"id": page_id, "name": name})
        self.model = ET.SubElement(
            self.diagram,
            "mxGraphModel",
            {
                "dx": "1800",
                "dy": "1200",
                "grid": "1",
                "gridSize": "10",
                "guides": "1",
                "tooltips": "1",
                "connect": "1",
                "arrows": "1",
                "fold": "1",
                "page": "1",
                "pageScale": "1",
                "pageWidth": str(width),
                "pageHeight": str(height),
                "math": "0",
                "shadow": "0",
            },
        )
        self.root = ET.SubElement(self.model, "root")
        ET.SubElement(self.root, "mxCell", {"id": "0"})
        ET.SubElement(self.root, "mxCell", {"id": "1", "parent": "0"})
        self.prefix = page_id + "_"

    def shape(self, key, value, x, y, w, h, style, parent="1"):
        cell = ET.SubElement(
            self.root,
            "mxCell",
            {
                "id": self.prefix + key,
                "value": value,
                "style": style,
                "vertex": "1",
                "parent": self.prefix + parent if parent != "1" else "1",
            },
        )
        ET.SubElement(
            cell,
            "mxGeometry",
            {"x": str(x), "y": str(y), "width": str(w), "height": str(h), "as": "geometry"},
        )
        return key

    def edge(self, key, source, target, value="", style=None):
        if style is None:
            style = f"edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeColor={INK};strokeWidth=2;endArrow=open;endFill=0;"
        cell = ET.SubElement(
            self.root,
            "mxCell",
            {
                "id": self.prefix + key,
                "value": value,
                "style": style,
                "edge": "1",
                "parent": "1",
                "source": self.prefix + source,
                "target": self.prefix + target,
            },
        )
        ET.SubElement(cell, "mxGeometry", {"relative": "1", "as": "geometry"})


ACTOR = f"shape=umlActor;html=1;verticalLabelPosition=bottom;verticalAlign=top;align=center;fillColor={INK};strokeColor={INK};fontColor={INK};fontSize=21;fontStyle=1;"
SYSTEM = f"swimlane;html=1;startSize=48;horizontal=1;rounded=0;fillColor={PANEL};swimlaneFillColor=#FFFFFF;strokeColor={BLUE};strokeWidth=2;align=center;fontColor={INK};fontSize=24;fontStyle=1;container=1;collapsible=0;"
USECASE = f"ellipse;whiteSpace=wrap;html=1;align=center;verticalAlign=middle;fillColor={PALE};strokeColor={BLUE};strokeWidth=2;fontColor={INK};fontSize=18;spacing=8;"
NOTE = f"shape=note;whiteSpace=wrap;html=1;align=left;verticalAlign=middle;fillColor=#FFF7DF;strokeColor=#A78D42;fontColor={INK};fontSize=16;spacingLeft=14;spacingRight=12;"
ASSOC = f"edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeColor={INK};strokeWidth=2;endArrow=none;startArrow=none;"
INCLUDE = f"edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeColor={BLUE};strokeWidth=2;dashed=1;dashPattern=7 5;endArrow=open;endFill=0;fontColor={INK};fontSize=15;labelBackgroundColor=#FFFFFF;"
DEPENDENCY = f"edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeColor={BLUE};strokeWidth=2;dashed=1;dashPattern=7 5;endArrow=open;endFill=0;fontColor={INK};fontSize=15;labelBackgroundColor=#FFFFFF;"
PACKAGE = f"swimlane;html=1;startSize=42;horizontal=1;rounded=0;fillColor={PANEL};swimlaneFillColor=#FFFFFF;strokeColor={BLUE};strokeWidth=2;align=center;fontColor={INK};fontSize=21;fontStyle=1;container=1;collapsible=0;"
COMPONENT = f"shape=component;whiteSpace=wrap;html=1;align=center;verticalAlign=middle;fillColor={ACCENT};strokeColor={BLUE};strokeWidth=2;fontColor={INK};fontSize=18;spacing=8;"
ARTIFACT = f"shape=note;whiteSpace=wrap;html=1;align=center;verticalAlign=middle;fillColor=#E2EDF8;strokeColor={BLUE};strokeWidth=2;fontColor={INK};fontSize=18;spacing=8;"
DEVICE = f"swimlane;html=1;startSize=52;horizontal=1;rounded=0;fillColor={PANEL};swimlaneFillColor=#FFFFFF;strokeColor={BLUE};strokeWidth=3;align=center;fontColor={INK};fontSize=22;fontStyle=1;container=1;collapsible=0;"
EXECUTION = f"swimlane;html=1;startSize=50;horizontal=1;rounded=0;fillColor=#F7FBFE;swimlaneFillColor=#FFFFFF;strokeColor={BLUE};strokeWidth=2;align=center;fontColor={INK};fontSize=20;fontStyle=1;container=1;collapsible=0;"


file = ET.Element("mxfile", {"host": "app.diagrams.net", "modified": "2026-10-29T00:00:00.000Z", "agent": "RepairDesk UML", "version": "24.7.17", "type": "device", "pages": "3"})

# 1. Use cases.  "Вести учёт клиента и заказа" is the end-to-end process;
# its included decision is recorded inside the selected order, not in a separate tab.
p = Page(file, "usecases", "1 Варианты использования", 2000, 1180)
p.shape("system", "Система учёта ремонта компьютерной техники", 260, 40, 1700, 1080, SYSTEM)
p.shape("reception", "Приёмщик", 65, 175, 130, 155, ACTOR)
p.shape("technician", "Мастер", 65, 600, 130, 155, ACTOR)
p.shape("admin", "Администратор", 60, 925, 140, 155, ACTOR)

p.shape("account", "Вести учёт клиента<br>и заказа", 330, 115, 340, 95, USECASE, "system")
p.shape("create", "Оформить заказ", 780, 85, 310, 90, USECASE, "system")
p.shape("register", "Зарегистрировать клиента<br>и устройство", 1240, 60, 330, 90, USECASE, "system")
p.shape("validate", "Проверить данные<br>и присвоить номер", 1240, 165, 330, 90, USECASE, "system")
p.shape("decision", "Зафиксировать<br>решение клиента", 780, 265, 310, 90, USECASE, "system")
p.shape("issue", "Выдать устройство", 780, 410, 310, 90, USECASE, "system")
p.shape("print", "Распечатать акт<br>приёма или выдачи", 330, 405, 340, 90, USECASE, "system")
p.shape("route_note", "Путь в интерфейсе:<br>«Заказы» → выбор заказа → карточка.<br>После диагностики приёмщик выбирает<br>«Клиент согласен» или «Клиент отказался».", 1195, 280, 380, 125, NOTE, "system")

p.shape("diagnose", "Сохранить диагностику<br>и план работ", 330, 575, 340, 95, USECASE, "system")
p.shape("work", "Добавить выполненную<br>работу", 790, 560, 310, 90, USECASE, "system")
p.shape("part", "Списать запчасть<br>в заказ", 790, 705, 310, 90, USECASE, "system")
p.shape("stock", "Проверить и пересчитать<br>остаток", 1240, 705, 330, 90, USECASE, "system")
p.shape("ready", "Отметить готовность<br>к выдаче", 330, 720, 340, 90, USECASE, "system")
p.shape("catalog", "Добавить запчасть<br>в каталог", 330, 905, 340, 90, USECASE, "system")
p.shape("summary", "Просмотреть сводку<br>по заказам", 790, 905, 310, 90, USECASE, "system")
p.shape("admin_note", "Администратор также может выполнять<br>операции приёмщика и мастера.", 1225, 905, 350, 90, NOTE, "system")

for key, src, dst in [
    ("a_rec_account", "reception", "account"),
    ("a_rec_print", "reception", "print"),
    ("a_master_diag", "technician", "diagnose"),
    ("a_master_work", "technician", "work"),
    ("a_master_part", "technician", "part"),
    ("a_master_ready", "technician", "ready"),
    ("a_admin_catalog", "admin", "catalog"),
    ("a_admin_summary", "admin", "summary"),
]:
    p.edge(key, src, dst, style=ASSOC)
for key, src, dst in [
    ("i_account_create", "account", "create"),
    ("i_account_decision", "account", "decision"),
    ("i_account_issue", "account", "issue"),
    ("i_create_register", "create", "register"),
    ("i_create_validate", "create", "validate"),
    ("i_part_stock", "part", "stock"),
]:
    p.edge(key, src, dst, "«include»", INCLUDE)

# 2. Components.  All named application components correspond to actual source
# files or a clearly identified external library, rather than a fictitious server.
p = Page(file, "components", "2 Компоненты", 1800, 1050)
p.shape("ui", "Представление и навигация", 55, 70, 410, 615, PACKAGE)
p.shape("app", "Прикладная логика", 500, 70, 780, 615, PACKAGE)
p.shape("storage", "Хранение данных", 1320, 70, 410, 615, PACKAGE)
p.shape("domain_pkg", "Предметная область", 500, 745, 780, 235, PACKAGE)
p.shape("print_pkg", "Средства Windows", 1320, 745, 410, 235, PACKAGE)

p.shape("view", "MainWindow.xaml<br>WPF: вкладки и карточка заказа", 45, 115, 320, 115, COMPONENT, "ui")
p.shape("vm", "MainViewModel<br>команды и состояние", 45, 340, 320, 110, COMPONENT, "ui")
p.shape("order", "OrderService<br>заказ и решение клиента", 55, 100, 310, 110, COMPONENT, "app")
p.shape("inventory", "InventoryService<br>запчасти и остаток", 425, 100, 310, 110, COMPONENT, "app")
p.shape("access", "AccessService<br>проверка роли", 55, 270, 310, 110, COMPONENT, "app")
p.shape("summary", "SummaryService<br>сводка", 425, 270, 310, 110, COMPONENT, "app")
p.shape("documents", "DocumentService<br>акты приёма и выдачи", 240, 450, 310, 110, COMPONENT, "app")
p.shape("context", "RepairDbContext<br>EF Core контекст", 45, 100, 320, 105, COMPONENT, "storage")
p.shape("provider", "EF Core + SQLite provider<br>библиотеки .NET", 45, 270, 320, 110, COMPONENT, "storage")
p.shape("db", "«artifact» repair.db<br>%LOCALAPPDATA%\\RepairDesk", 45, 445, 320, 105, ARTIFACT, "storage")
p.shape("entities", "Customer · Device · RepairOrder<br>RepairWork · SparePart · PartUsage", 115, 65, 550, 105, COMPONENT, "domain_pkg")
p.shape("printer", "PrintDialog / Windows Print Spooler", 45, 70, 320, 105, COMPONENT, "print_pkg")

for key, src, dst, label in [
    ("d_view_vm", "view", "vm", "команды"),
    ("d_vm_order", "vm", "order", "создание, согласование, выдача"),
    ("d_vm_inventory", "vm", "inventory", "склад"),
    ("d_vm_summary", "vm", "summary", "показатели"),
    ("d_vm_docs", "vm", "documents", "печать"),
    ("d_order_access", "order", "access", "права"),
    ("d_inventory_access", "inventory", "access", "права"),
    ("d_order_context", "order", "context", "данные"),
    ("d_inventory_context", "inventory", "context", "данные"),
    ("d_summary_context", "summary", "context", "чтение"),
    ("d_context_provider", "context", "provider", "ORM"),
    ("d_provider_db", "provider", "db", "SQLite"),
    ("d_order_domain", "order", "entities", "сущности"),
    ("d_inventory_domain", "inventory", "entities", "сущности"),
    ("d_documents_printer", "documents", "printer", "диалог печати"),
]:
    p.edge(key, src, dst, label, DEPENDENCY)

# 3. Deployment.  SQLite is a local file in the Windows profile, not a
# database server and not an artifact running inside the .NET environment.
p = Page(file, "deployment", "3 Развёртывание", 1800, 1080)
p.shape("workstation", "«device» Рабочее место сотрудника", 65, 65, 1290, 900, DEVICE)
p.shape("windows", "«executionEnvironment» Windows 10/11", 60, 110, 1170, 700, EXECUTION, "workstation")
p.shape("dotnet", "«executionEnvironment» .NET 8 Desktop Runtime", 70, 125, 650, 455, EXECUTION, "windows")
p.shape("exe", "«artifact»<br>RepairDesk.exe<br>запуск приложения", 50, 120, 250, 125, ARTIFACT, "dotnet")
p.shape("dll", "«artifact»<br>RepairDesk.dll<br>код WPF и сервисов", 350, 120, 250, 125, ARTIFACT, "dotnet")
p.shape("libs", "«artifact»<br>EF Core / SQLite provider DLLs", 185, 310, 290, 105, ARTIFACT, "dotnet")
p.shape("db", "«artifact» repair.db<br>%LOCALAPPDATA%\\RepairDesk<br>локальный файл SQLite", 785, 165, 310, 145, ARTIFACT, "windows")
p.shape("spooler", "«executionEnvironment»<br>Windows Print Spooler", 785, 440, 310, 120, EXECUTION + "fontSize=16;startSize=76;", "windows")
p.shape("single_pc", "Приложение и база размещены на одном ПК.<br>Отдельного сервера и сетевой БД нет.", 230, 815, 760, 70, NOTE, "workstation")
p.shape("printer", "«device» Принтер", 1485, 425, 240, 195, DEVICE)
p.shape("print_job", "Печать актов<br>приёма и выдачи", 20, 78, 200, 85, ARTIFACT, "printer")
p.edge("e_exe_dll", "exe", "dll", "запускает", DEPENDENCY)
p.edge("e_dll_libs", "dll", "libs", "использует", DEPENDENCY)
p.edge("e_libs_db", "libs", "db", "локальный файловый доступ", DEPENDENCY)
p.edge("e_exe_spooler", "exe", "spooler", "печать", DEPENDENCY)
p.edge("e_spooler_printer", "spooler", "printer", "USB / сеть", ASSOC)

ET.indent(file, space="  ")
ET.ElementTree(file).write(OUTPUT, encoding="utf-8", xml_declaration=True)
print(OUTPUT)
