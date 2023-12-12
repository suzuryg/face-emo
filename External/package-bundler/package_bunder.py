"""
MIT License

Copyright (c) 2023 suzuryg

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
"""

import os
import sys
import time
import logging
import importlib.util
from pathlib import Path

def read_config(config_file):
    spec = importlib.util.spec_from_file_location("config", config_file)
    config = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(config)
    return config

def create_symlink(src, dst, symlink_logger):
    if dst.is_symlink():
        suffix = 1
        while dst.with_name(f"{dst.stem}_{suffix}{dst.suffix}").is_symlink():
            suffix += 1
        dst = dst.with_name(f"{dst.stem}_{suffix}{dst.suffix}")

    dst.symlink_to(os.path.relpath(src, dst.parent))
    symlink_logger.info(f'Created symbolic link: {dst}')

def create_gitignore(dst, content):
    dst.mkdir(parents=True, exist_ok=True)
    with open(dst / '.gitignore', 'w') as file:
        file.write(content)

def package_bundler(src_dir, dst_dir, config, symlink_logger, ignored_hierarchy_logger, ignored_file_logger):
    for root, dirs, files in os.walk(src_dir, topdown=True):
        if any(Path(root).relative_to(src_dir).parts[:len(dir_.parts)] == dir_.parts for dir_ in config.ignore_dirs):
            dirs[:] = []
            ignored_hierarchy_logger.info(f'Ignored hierarchy: {root}')
            continue

        for file in files:
            if file.endswith('.meta'):
                continue
            elif not any(file.endswith(ext) for ext in config.target_extensions):
                ignored_file_logger.info(f'Ignored file: {root}/{file}')
                continue
            elif any(file == to_ignore for to_ignore in config.ignore_files):
                ignored_file_logger.info(f'Ignored file: {root}/{file}')
                continue

            create_symlink(Path(root) / file, dst_dir / file, symlink_logger)

        if any(file.endswith(ext) for ext in config.target_extensions for file in files):
            create_gitignore(dst_dir, config.gitignore_content)

if __name__ == "__main__":
    config_file = sys.argv[1]
    config = read_config(config_file)

    log_dir = Path('log')
    log_dir.mkdir(parents=True, exist_ok=True)

    timestamp = time.strftime("%Y%m%d%H%M%S")

    symlink_logger = logging.getLogger('symlink')
    symlink_logger.setLevel(logging.INFO)
    symlink_logger.addHandler(logging.FileHandler(log_dir / f'{Path(config_file).stem}_{timestamp}_symlink.log'))

    ignored_hierarchy_logger = logging.getLogger('ignored_hierarchy')
    ignored_hierarchy_logger.setLevel(logging.INFO)
    ignored_hierarchy_logger.addHandler(logging.FileHandler(log_dir / f'{Path(config_file).stem}_{timestamp}_ignored_hierarchy.log'))

    ignored_file_logger = logging.getLogger('ignored_file')
    ignored_file_logger.setLevel(logging.INFO)
    ignored_file_logger.addHandler(logging.FileHandler(log_dir / f'{Path(config_file).stem}_{timestamp}_ignored_file.log'))

    package_bundler(config.src_dir, config.dst_dir, config, symlink_logger, ignored_hierarchy_logger, ignored_file_logger)
