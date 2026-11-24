import csv

# with open("mahsulotlar.csv", "r", encoding="utf-8", newline="") as fayl:
#     reader = csv.DictReader(fayl)
#     for qator in reader:
#         print(qator)

# mahsulotlar=[]
# with open("mahsulotlar.csv", "r", encoding="utf-8", newline="") as fayl:             # csv dan ma'lumoltarni arrayga yuklash
#     for qator in csv.DictReader(fayl):                                               # if not qiymat.strip(): # bo'sh qatorlarga tekshirish
#         mahsulotlar.append({
#             "id": int(qator["id"]),
#             "nom": qator["nom"],
#             "narx": int(qator["narx"]),
#             "miqdor": int(qator["miqdor"]),
#             "kategoriya": qator["kategoriya"],
#         })
#
# print(mahsulotlar)
#
# ombor_qiymati = sum(
#     mahsulot["narx"] * mahsulot["miqdor"]
#     for mahsulot in mahsulotlar
# )
#
# print(ombor_qiymati)


# yangi_mahsulotlar = [                                                             # yangi csv yaratish
#     {"nom": "Banan", "narx": 15000, "miqdor": 10},
#     {"nom": "Qattiq", "narx": 7000, "miqdor": 6}
# ]
#
# with open("yangi_mahsulotlar.csv", "w", encoding="utf-8", newline="") as fayl:
#     ustunlar = ["nom", "narx", "miqdor"]
#     writer = csv.DictWriter(fayl, fieldnames=ustunlar)
#
#     writer.writeheader()
#     writer.writerows(yangi_mahsulotlar)

