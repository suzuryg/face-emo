from pathlib import Path

src_dir = Path('../Extenject/AssemblyBuild/Zenject-usage')
dst_dir = Path('../../Packages/jp.suzuryg.face-emo/Ext/Zenject/a/u')
ignore_dirs = []
ignore_files = ['JetbrainsAnnotations.cs']
target_extensions = ['.cs',]
gitignore_content = '''*.cs
*.meta
!*.asmdef.meta
'''
