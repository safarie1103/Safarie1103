import kagglehub

# Download latest version
path = kagglehub.dataset_download("ryanholbrook/car-or-truck")

print("Path to dataset files:", path)