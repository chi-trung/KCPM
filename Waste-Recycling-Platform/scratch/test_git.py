import subprocess
try:
    res = subprocess.run(["git", "status"], capture_output=True, text=True, check=True)
    print(res.stdout)
except Exception as e:
    print(f"Error: {e}")
