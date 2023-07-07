import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';

function Features() {
  return (
    <div className="row">
      <div className={clsx('col col-4')}>
        <div className="text--center">
          <img src={require('@site/static/img/root_page/01.png').default} className={styles.feature_img} />
        </div>
        <div className="text--center padding-horiz--md">
          <h3>かんたん表情作成</h3>
          <p>
            トリガーウィンクもサクッと完了。<br/>
            リップシンクのオンオフもボタン一つ。<br/>
            もちろん、編集もらくちん。
          </p>
        </div>
      </div>
      <div className={clsx('col col-4')}>
        <div className="text--center">
          <img src={require('@site/static/img/root_page/02.png').default} className={styles.feature_img} />
        </div>
        <div className="text--center padding-horiz--md">
          <h3>らくらく管理</h3>
          <p>
            表情パターンをリスト表示。<br/>
            ジェスチャーの組み合わせも表管理。
          </p>
        </div>
      </div>
      <div className={clsx('col col-4')}>
        <div className="text--center">
          <img src={require('@site/static/img/root_page/03.png').default} className={styles.feature_img} />
        </div>
        <div className="text--center padding-horiz--md">
          <h3>切り替え自由</h3>
          <p>
            表情パターンは Ex Menu で切り替え簡単。<br/>
            TPOで使い分け！
          </p>
        </div>
      </div>
    </div>
  );
}


function Details ()  {
  return (
    <div>
      <div className="text--center" >
        <h1 className="homepage_h1_padding" >FaceEmo機能一覧</h1>
      </div>
      <div className="row">
        <div className={clsx('col col--4')}>
          <div className="text--right">
            <img src={require('@site/static/img/root_page/function01.png').default} className={styles.feature_detail_img} />
          </div>
        </div>
        <div className={clsx('col col-8')}>
          <div className="text--left">
            表情エディター
          </div>
        </div>
      </div>
      <div className="row">
        <div className={clsx('col col--4')}>
          <div className="text--right">
            <img src={require('@site/static/img/root_page/function02.png').default} className={styles.feature_detail_img} />
          </div>
        </div>
        <div className={clsx('col col-8')}>
          <div className="text--left">
            左右ジェスチャーを組み合わせの一覧表
          </div>
        </div>
      </div>
      <div className="row">
        <div className={clsx('col col--4')}>
          <div className="text--right">
            <img src={require('@site/static/img/root_page/function03.png').default} className={styles.feature_detail_img} />
          </div>
        </div>
        <div className={clsx('col col-8')}>
          <div className="text--left">
            表情パターンごとにデフォルト表情を変えられる
          </div>
        </div>
      </div>
    </div>
  );
}

function Install() {
  return(
    <div>
      <div className="text--center">
        <h1 className="homepage_h1_padding_0rem">インストール</h1>
      </div>
      <div className="row">
        {/*
        <div className={clsx('col col--4')}>
          <div className="text--right">
            <img src={require('@site/static/img/root_page/01.png').default} className={styles.feature_img}/>
          </div>
        </div>
        */}
        <div className={clsx('col col--3 padding-horiz--md')}></div>
        <div className={clsx('col col--8')}>
          <div className="text--left">
            1. [VRChat Creator Companion](https://vcc.docs.vrchat.com/)を導入<br/>
            2. Modular Avatar の リポジトリを登録<br/>
            3. FaceEmo のリポジトリを登録<br/>
            4. プロジェクトに最新版を設定<br/>
            [unitypackage版](https://github.com/suzuryg/face-emo/releases)<br/>
          </div>
        </div>
        <div className={clsx('col col--3')}></div>
      </div>
    </div>
  )
}

function DependentSoftware() {
  return(
    <div>
      <div className="text--center">
        <h1 className="homepage_h1_padding_0rem">関連ソフトウェア</h1>
      </div>
      <div className="row">
        <div className={clsx('col col--3 padding-horiz--md')}></div>
        <div className={clsx('col col--8')}>
          <div className="text--left">
            - [ComboGestureExpression (Integrator)](https://booth.pm/ja/items/2219616)<br/>
            - [CustomAnimatorController](https://booth.pm/ja/items/4424448)<br/>
            - [Modular Avatar](https://modular-avatar.nadena.dev/ja/)<br/>
          </div>
        </div>
        <div className={clsx('col col--3')}></div>
      </div>
    </div>
  );
}

function ReleaseNote(){
  return(<div></div>);
}

function Author(){
  return(<div></div>);
}

export default function HomepageFeatures(): JSX.Element {
  return (
    <section className={styles.features}>
      <div className="container">
        <Features/>
        <hr/>
        <Details/>
        <hr/>
        <Install/>
        <hr/>
        <DependentSoftware/>
        <hr/>
        <ReleaseNote/>
        <Author/>
      </div>
    </section>
  );
}
