import matplotlib.pyplot as plt
import pandas as pd

# x = [1, 2, 3, 4]
# y = [10, 25, 18, 30]
#
# fig, ax = plt.subplots()
#
# ax.plot(x, y)
# plt.show()

# oylar = ["Yanvar", "Fevral", "Mart", "Aprel"]  # sarlavha, o'qlar va kataklar
# sotuv = [120, 150, 100, 180]
#
# fig, ax = plt.subplots()
#
# ax.plot(oylar, sotuv, marker="o")
# ax.set_title("Oylik sotuv")
# ax.set_xlabel("Oy")
# ax.set_ylabel("Sotuv soni")
# ax.grid(True)
#
# plt.show()
#
# kunlar = [1, 2, 3, 4, 5]                         # chiziqli grafika
# harorat = [18, 20, 17, 23, 25]
#
# fix, ax = plt.subplots()
#
# ax.plot(kunlar, harorat, color="green", marker="o", linestyle="--")
# ax.set_title("5 kunlik harorat")
# ax.set_xlabel("Kun")
# ax.set_ylabel("Harorat (°C)")
# ax.grid(True)
#
# plt.show()

# fanlar = ["Matematika", "Python", "Ingiliz tili"]                   # ustunli grafika
# ballar = [85, 92, 78]
#
# fig, ax = plt.subplots()
#
# ax.bar(fanlar, ballar, color="skyblue")
# ax.set_title("Fanlar bo'yicha ballar")
# ax.set_xlabel("Fan")
# ax.set_ylabel("Ball")
# ax.set_ylim(0, 100)
#
# plt.show()

# oqish_soati = [1, 2, 3, 4, 5]
# ball = [55, 60, 72, 80,92]
#
# fix, ax = plt.subplots()
# ax.scatter(oqish_soati, ball, color="red")
# ax.set_title("O'qish va imtihonlar ballari")
# ax.set_xlabel("O'qish soati")
# ax.set_ylabel("Imtihon ballari")
# ax.grid(True)
#
# plt.show()

# fanlar = ["Matematika", "Python", "Ingliz tili"]                          # Diagrammali grafika
# vaqt = [5, 8, 3]
#
# fig, ax = plt.subplots()
#
# ax.pie(vaqt, labels=fanlar, autopct="%1.1f%%")
# ax.set_title("O‘qish vaqtining taqsimoti")
#
# plt.show()

# oylar = [1, 2, 3, 4]
# sotuv = [100, 130, 120, 160]
# xarajat = [70, 80, 90, 100]
#
# fig, axs = plt.subplots(1, 2, figsize=(10, 4))
#
# axs[0].plot(oylar, sotuv, marker="o")
# axs[0].set_title("Sotuv")
# axs[0].set_xlabel("Oy")
#
# axs[1].bar(oylar, xarajat, color="orange")
# axs[1].set_title("Xarajat")
# axs[1].set_xlabel("Oy")
#
# plt.tight_layout()
# plt.show()

# kunlar = [1, 2, 3]                                               # Grafikni faylga saqlash
# harorat = [20, 22, 19]
#
# fig, ax = plt.subplots()
# ax.plot(kunlar, harorat, marker="o")
# ax.set_title("Harorat")
#
# fig.savefig("harorat.svg", dpi=150)
# plt.show()

# df = pd.DataFrame({                                                      # Pandas bilan ishlatish
#     "Oy": ["Yanvar", "Fevral", "Mart", "Aprel"],
#     "Sotuv": [120, 150, 100, 180]
# })
#
# fig, ax = plt.subplots()
#
# ax.plot(df["Oy"], df["Sotuv"], marker="o")
# ax.set_title("Oylik sotuv")
# ax.set_xlabel("Oy")
# ax.set_ylabel("Sotuv")
#
# plt.show()