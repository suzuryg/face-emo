from pathlib import Path

src_dir = Path('../av3-animator-as-code/Framework/Editor/V0')
dst_dir = Path('../../Packages/jp.suzuryg.face-emo/Ext/AnimatorAsCodeFramework')
ignore_dirs = []
ignore_files = []
target_extensions = ['.cs',]
gitignore_content = '''*.cs
*.meta
!*.asmdef.meta
'''
