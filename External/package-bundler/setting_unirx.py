from pathlib import Path

src_dir = Path('../UniRx/Assets/Plugins/UniRx/Scripts')
dst_dir = Path('../../Packages/jp.suzuryg.face-emo/Ext/UniRx/Runtime')
ignore_dirs = []
ignore_files = []
target_extensions = ['.cs',]
gitignore_content = '''*.cs
*.meta
!*.asmdef.meta
'''
