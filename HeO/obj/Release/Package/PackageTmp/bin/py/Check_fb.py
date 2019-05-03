from selenium import webdriver
from selenium.webdriver.firefox.firefox_binary import FirefoxBinary
from selenium.webdriver.common.keys import Keys
import sys

FB_account = sys.argv[1]
FB_password =  sys.argv[2]
binary = FirefoxBinary(r'C:\Program Files\Mozilla Firefox\firefox.exe')
profile = webdriver.FirefoxProfile()
profile.set_preference('permissions.default.desktop-notification', 1)
drive = webdriver.Firefox(firefox_binary=binary,firefox_profile=profile)
drive.get('https://www.facebook.com/') #電腦版
drive.find_element_by_id('email').send_keys(FB_account)   #登入帳號
drive.find_element_by_id('pass').send_keys(FB_password)   #登入密碼
drive.find_element_by_id("loginbutton").click()           #登入按鈕

try:
    drive.find_element_by_name('login')
    status = "failed"
except:
    try:
        drive.find_element_by_name('submit[Secure Account]')
        status = "failed"
    except:
        status = "success"

driver.quit()
sys.stdout.write(status)
sys.stdout.flush()
sys.exit(0)
