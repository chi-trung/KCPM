import sys
import subprocess

try:
    import yaml
except Exception:
    subprocess.check_call([sys.executable, '-m', 'pip', 'install', 'PyYAML'])
    import yaml

path = '.github/workflows/allure-gh-pages.yml'
try:
    with open(path, 'r', encoding='utf8') as f:
        yaml.safe_load(f)
    print('YAML OK')
except Exception as e:
    print('YAML ERROR')
    raise
