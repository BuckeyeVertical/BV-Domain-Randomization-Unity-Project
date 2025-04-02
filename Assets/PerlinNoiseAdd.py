from PIL import Image
import numpy as np
import os
import shutil
import random

def add_noise_to_image(image_path, output_path, noise_intensity=1.0):
    # Ensure the noise intensity is within the allowed range
    noise_intensity = max(0, min(noise_intensity, 1))
    
    # Load the image
    image = Image.open(image_path)
    image_array = np.array(image)
    
    # Generate noise
    noise = np.random.randint(0, 256, image_array.shape, dtype=np.uint8)

    # Combine the image and noise, adjusting the intensity of the noise
    noisy_image_array = image_array + noise * (0.5 * noise_intensity)
    noisy_image_array = np.clip(noisy_image_array, 0, 255)  # Ensure values stay in the 0-255 range

    # Convert back to an image
    noisy_image = Image.fromarray(noisy_image_array.astype(np.uint8))
    noisy_image.save(output_path)

def add_noise_to_directory(input_dir, output_dir):
    # Ensure output directory exists
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)

    # Get all image files
    image_files = [f for f in os.listdir(input_dir) if f.lower().endswith(('.png', '.jpg', '.jpeg'))]

    # Determine the number of images to randomly select for noise addition
    num_to_select = int(len(image_files) // 4)

    # Randomly select about 1/3 of the images to add noise to
    selected_files = random.sample(image_files, num_to_select)

    # Iterate over all files, add noise to selected ones, and copy others
    for filename in image_files:
        input_path = os.path.join(input_dir, filename)
        output_path = os.path.join(output_dir, filename)
        if filename in selected_files:
            # Generate a random intensity for each image
            noise_intensity = random.uniform(0, 0.5)
            print(f"Adding noise to: {filename} with intensity: {noise_intensity}")
            add_noise_to_image(input_path, output_path, noise_intensity)
        else:
            print(f"Copying without adding noise to: {filename}")
            shutil.copy(input_path, output_path)
    print("Processing complete.")

# Example usage: Adjust 'input_directory' and 'output_directory' to your paths
input_directory = "D:\\Data\\Buckeye Vertical\\Image Classifier\\train\\images"
output_directory = "D:\\Data\\Buckeye Vertical\\Image Classifier\\train\\imagesnoise"

add_noise_to_directory(input_directory, output_directory)
