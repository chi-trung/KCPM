import json
import os


def main():
    out_dir = 'validation-temp'
    src = 'Waste-Recycling-Platform/allure-results/jira-owner-map.json'
    os.makedirs(out_dir, exist_ok=True)

    summary = {'resolved': 0, 'total': 0, 'sample': {}}
    if os.path.exists(src):
        with open(src, 'r', encoding='utf8') as f:
            try:
                m = json.load(f)
            except Exception:
                m = {}
        summary['total'] = len(m)
        for k, v in list(m.items())[:20]:
            if v and not v.get('unassigned'):
                summary['resolved'] += 1
            summary['sample'][k] = v

    with open(os.path.join(out_dir, 'summary.json'), 'w', encoding='utf8') as f:
        json.dump(summary, f, ensure_ascii=False, indent=2)

    print('Wrote validation summary to', os.path.join(out_dir, 'summary.json'))


if __name__ == '__main__':
    main()
