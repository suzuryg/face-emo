import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';
import Translate from '@docusaurus/Translate'

function Features() {
  return (
    <div className="row">
      <div className={clsx('col col-4')}>
        <div className="text--center">
          <img src={require('@site/static/img/root_page/01.png').default} className={styles.feature_img} />
        </div>
        <div className="text--center padding-horiz--md">
          <h3><Translate>Effortless Emote Creation</Translate></h3>
          <p>
            <Translate>Easily create trigger winks.</Translate><br/>
            <Translate>Switch lip sync on and off with a single click.</Translate><br/>
            <Translate>Of course, editing is a breeze.</Translate>
          </p>
        </div>
      </div>
      <div className={clsx('col col-4')}>
        <div className="text--center">
          <img src={require('@site/static/img/root_page/02.png').default} className={styles.feature_img} />
        </div>
        <div className="text--center padding-horiz--md">
          <h3><Translate>Easy Management</Translate></h3>
          <p>
            <Translate>List display for facial expression patterns.</Translate><br/>
            <Translate>Manage combinations of gestures in a table.</Translate>
          </p>
        </div>
      </div>
      <div className={clsx('col col-4')}>
        <div className="text--center">
          <img src={require('@site/static/img/root_page/03.png').default} className={styles.feature_img} />
        </div>
        <div className="text--center padding-horiz--md">
          <h3><Translate>Freedom to Switch</Translate></h3>
          <p>
            <Translate>Easily switch between facial expression patterns.</Translate><br/>
            <Translate>Use different expressions for different occasions!</Translate>
          </p>
        </div>
      </div>
    </div>
  );
}

function Credits(){
  return(
    <div>
      <div className="row">
        <div className={clsx('col col--3 padding-horiz--md')}></div>
        <div className={clsx('col col--8')}>
          <div className="text--left">
            <Translate>The images in this document are made using the following tools and assets.</Translate>
            <ul>
              <li><a href="https://booth.pm/ja/items/4667400/"><Translate>Moe avatar (by Kyubi closet)</Translate></a></li>
              <li><a href="https://github.com/BlackStartx/VRC-Gesture-Manager/"><Translate>Gesture Manager (by BlackStartx)</Translate></a></li>
            </ul>
          </div>
        </div>
        <div className={clsx('col col--3')}></div>
      </div>
    </div>);
}

export default function HomepageFeatures(): JSX.Element {
  return (
    <section className={styles.features}>
      <div className="container">
        <Features/>
        <hr/>
        <Credits/>
      </div>
    </section>
  );
}
