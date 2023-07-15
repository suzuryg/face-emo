from pathlib import Path

src_dir = Path('../Extenject/UnityProject/Assets/Plugins/Zenject/Source')
dst_dir = Path('../../Packages/jp.suzuryg.face-emo/Ext/Zenject/a')
ignore_dirs = [Path('Editor'),]
target_extensions = ['.cs',]
gitignore_content = '''*.cs
*.meta
!*.asmdef.meta
'''
