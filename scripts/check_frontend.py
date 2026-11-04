"""Comprobación estructural del frontend, sin herramientas de terceros."""
from html.parser import HTMLParser
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "frontend"
PAGE = ROOT / "index.html"

class PageParser(HTMLParser):
    def __init__(self):
        super().__init__()
        self.ids = set()
        self.paths = []
    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if "id" in attrs:
            self.ids.add(attrs["id"])
        if tag in ("script", "link"):
            value = attrs.get("src" if tag == "script" else "href")
            if value and value.startswith("./"):
                self.paths.append(value)

def main():
    page = PageParser()
    page.feed(PAGE.read_text(encoding="utf-8"))
    assert {"api-status", "retry-api"} <= page.ids, "Faltan controles de API"
    missing = [item for item in page.paths if not (ROOT / item).is_file()]
    assert not missing, f"Recursos inexistentes: {missing}"
    print("Frontend: HTML y recursos locales correctos")

if __name__ == "__main__":
    main()
