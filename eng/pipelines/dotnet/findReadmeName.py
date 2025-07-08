import os
import re

def extract_readme_names():
    # Get the directory of the current script
    current_dir = os.path.dirname(os.path.abspath(__file__))
    # Locate the package folder
    package_dir = os.path.join(current_dir, "package")
    
    # Check if the package folder exists
    if not os.path.exists(package_dir) or not os.path.isdir(package_dir):
        print("Package folder not found.")
        return
    
    readme_names = []
    
    # Traverse all files in the package folder
    for root, _, files in os.walk(package_dir):
        for file in files:
            file_path = os.path.join(root, file)
            # Process only text files
            if file.endswith(".yml"):
                try:
                    with open(file_path, "r", encoding="utf-8") as f:
                        content = f.read()
                        # Use a regular expression to extract the value of ReadmeName:
                        # matches = re.findall(r"ReadmeName:\s*(.+)", content)
                        # matches = re.findall(r"\sPackageName:\s*(.+)", content)
                        matches = re.findall(r"CsvPackageName:\s*(.+)", content)
                        readme_names.extend(matches)
                except Exception as e:
                    print(f"Error reading file {file_path}: {e}")
    
    # Output the results
    for name in readme_names:
        print(name.replace('\'', '"').strip()+ ',')

if __name__ == "__main__":
    extract_readme_names()