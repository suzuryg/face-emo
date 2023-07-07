import React from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import HomepageFeatures from '@site/src/components/HomepageFeatures';
import Translate from '@docusaurus/Translate'

import styles from './index.module.css';

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx('hero ', styles.heroBanner, ' homepage_header_padding')}>
      <div className="container">
        <img src={require("@site/static/img/root_page/title.png").default} />
        <p className="hero__subtitle">{siteConfig.tagline}</p>
        <div className={styles.buttons}>
            <Link
                className={`button button--secondary button--lg  ${styles.button}`}
                to="https://suzuryg.booth.pm/">
                <Translate>Download</Translate>
            </Link>
            <Link
                className={`button button--secondary button--lg ${styles.button}`}
                to="/docs/tutorials">
                <Translate>Documentation</Translate>
            </Link>
        </div>
      </div>
    </header>
  );
}

export default function Home() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title={`${siteConfig.title}`}
      description="Description will go into a meta tag in <head />">
      <HomepageHeader />
      <main>
          <HomepageFeatures />
      </main>
    </Layout>
  );
}
