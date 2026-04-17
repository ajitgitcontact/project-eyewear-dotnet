import styles from "@/styles/footer.module.css";

const infoLinks = [
  "About Us",
  "Shipping Information",
  "Returns Policy",
  "FAQs",
  "Privacy Policy",
  "Terms and Conditions",
  "Prescription (RX)",
  "Contact",
  "Wholesale",
];

export default function Footer() {
  return (
    <footer className={styles.footer}>
      <div className={styles.inner}>
        <div className={styles.signupSection}>
          <p className={styles.kicker}>Sign Up For Updates</p>
          <h3 className={styles.title}>Stay in sync with new drops and exclusive releases.</h3>
          <p className={styles.text}>
            Join the Eyewear Co. list for launch alerts, styling notes, and curated product updates.
          </p>
          <div className={styles.signupRow}>
            <input type="email" placeholder="Email Address" className={styles.input} />
            <button type="button" className={styles.button}>
              Sign Up
            </button>
          </div>
          <label className={styles.checkboxRow}>
            <input type="checkbox" />
            <span>I accept the privacy and cookies policies and agree to receive personalized communication.</span>
          </label>
          <div className={styles.socials}>
            <span>Instagram</span>
            <span>Facebook</span>
            <span>TikTok</span>
            <span>X</span>
          </div>
        </div>

        <div className={styles.linksSection}>
          <p className={styles.kicker}>More Information</p>
          <div className={styles.linksGrid}>
            {infoLinks.map((link) => (
              <a key={link} href="#" className={styles.link}>
                {link}
              </a>
            ))}
          </div>
        </div>
      </div>
    </footer>
  );
}
