from pathlib import Path

src_dir = Path('../UniRx/Assets/Plugins/UniRx/Scripts')
dst_dir = Path('../../Packages/jp.suzuryg.facial-expression-switcher/External/neuecc/UniRx/Runtime')
ignore_dirs = []
target_extensions = ['.cs',]
gitignore_content = '''*.cs
*.meta
!*.asmdef.meta
'''
