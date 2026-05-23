import json
with open("postman/WastePlatform API - Professional QA Suite.postman_collection.json", "r", encoding="utf-8") as f:
    data = json.load(f)
    for item in data.get("item", []):
        print(item.get("name"))
