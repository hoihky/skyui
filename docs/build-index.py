#!/usr/bin/env python3
"""Build doc/index.html in Spark-style portfolio layout (not doc-site chrome)."""
from pathlib import Path
import re

DOC = Path(__file__).resolve().parent
body = (DOC / "sdk-guide-body.html").read_text()

body = body.replace("class=\"sdk-card-grid\"", "class=\"pillar-grid\"")
body = body.replace("class=\"sdk-card\"", "class=\"pillar-card\"")
body = re.sub(
    r'<div class="pillar-card">\s*<strong>([^<]+)</strong>\s*<span>([^<]+)</span>\s*</div>',
    r'<article class="pillar-card"><h3>\1</h3><p>\2</p></article>',
    body,
)
body = body.replace("class=\"sdk-callout\"", "class=\"callout\"")
body = re.sub(
    r'<div class="callout">\s*<p>(.*?)</p>\s*</div>',
    r'<blockquote><p>\1</p></blockquote>',
    body,
    flags=re.DOTALL,
)
body = body.replace("class=\"sdk-diagram\"", "class=\"diagram-block\"")
body = re.sub(r"<h2>([^<]+)</h2>", r'<h2 class="section-title">\1</h2>', body)

toc = """
      <aside class="toc" aria-label="On this page">
        <p class="toc-title">On this page</p>
        <ul>
          <li><a href="#overview">Overview</a></li>
          <li><a href="#quick-start">Quick start</a></li>
          <li><a href="#packages">Package architecture</a></li>
          <li><a href="#theme-pipeline">Theme pipeline</a></li>
          <li><a href="#control-patterns">Control patterns</a></li>
          <li><a href="#namespaces">XAML namespaces</a></li>
          <li><a href="#tokens">Tokens &amp; palettes</a></li>
          <li><a href="#breakpoints">Breakpoints</a></li>
          <li><a href="#density-focus">Density &amp; focus</a></li>
          <li><a href="#catalog-essentials">Essentials catalog</a></li>
          <li><a href="#catalog-data">Data &amp; diagram</a></li>
          <li class="toc-h3"><a href="#prog-theme">Theme setup</a></li>
          <li class="toc-h3"><a href="#prog-styled">Styled primitives</a></li>
          <li class="toc-h3"><a href="#prog-mvvm">MVVM</a></li>
          <li class="toc-h3"><a href="#prog-validation">Validation</a></li>
          <li class="toc-h3"><a href="#prog-icons">Icons</a></li>
          <li class="toc-h3"><a href="#prog-navigation">Navigation</a></li>
          <li class="toc-h3"><a href="#prog-menus">Menus</a></li>
          <li class="toc-h3"><a href="#prog-layout">Layout</a></li>
          <li class="toc-h3"><a href="#prog-attached">Attached properties</a></li>
          <li class="toc-h3"><a href="#prog-mobile">Mobile primitives</a></li>
          <li class="toc-h3"><a href="#prog-data">Data grid &amp; filter</a></li>
          <li><a href="#testing">Testing</a></li>
          <li><a href="#demo">Demo gallery</a></li>
          <li><a href="#docs">Further reading</a></li>
        </ul>
      </aside>
"""

html = f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>SkyUI — Cross-platform UI library for .NET on Avalonia</title>
  <meta name="description" content="SkyUI is a cross-platform UI library for .NET built on Avalonia 12 — design system, essentials controls, data grid, and diagram modules.">
  <link rel="preconnect" href="https://fonts.googleapis.com">
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
  <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet">
  <link rel="stylesheet" href="assets/css/portfolio.css">
</head>
<body>
  <div class="site">
    <header class="topbar">
      <a href="index.html" class="brand">SkyUI</a>
      <nav class="topbar-nav" aria-label="Primary">
        <a href="#overview">Overview</a>
        <a href="#quick-start">Quick start</a>
        <a href="#packages">Architecture</a>
        <a href="#catalog-essentials">Controls</a>
        <a href="#prog-theme">Programming</a>
        <a href="#docs">Docs</a>
        <a href="https://github.com/hoihky/skyui" target="_blank" rel="noopener">GitHub</a>
      </nav>
    </header>

    <main class="portfolio-layout">
      <div class="portfolio-main doc-body">
{body}

        <footer class="site-footer">
          <p>
            <a href="https://github.com/hoihky/skyui" target="_blank" rel="noopener">SkyUI on GitHub</a>
            · <a href="https://avaloniaui.net/" target="_blank" rel="noopener">Avalonia</a>
            · <a href="https://hoihky.github.io/" target="_blank" rel="noopener">Hoi Kwan Yeung</a>
          </p>
        </footer>
      </div>
{toc}
    </main>
  </div>
</body>
</html>
"""

(DOC / "index.html").write_text(html)
print("Wrote index.html (portfolio layout)")
