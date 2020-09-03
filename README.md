# HitomiViewer

[![release latest](https://img.shields.io/github/release/rmagur1203/HitomiViewer.svg?logo=github)](https://github.com/rmagur1203/HitomiViewer/releases/latest)
[![download latest](https://img.shields.io/github/downloads/rmagur1203/HitomiViewer/latest/total.svg?logo=github)](https://github.com/rmagur1203/HitomiViewer/releases/latest)
[![downloads](https://img.shields.io/github/downloads/rmagur1203/HitomiViewer/total.svg?logo=github)](https://github.com/rmagur1203/HitomiViewer/releases)

업데이트는 실행 시 자동으로 되고 Updater.exe를 실행할 시에도 업데이트가 됩니다.
현재는 별도로 우회 프로그램을 사용하지 않아도 모든 기능이 사용이 가능합니다.

# Framework
.NET Framework 4.8 - [download](https://dotnet.microsoft.com/download) - [downloads](https://dotnet.microsoft.com/download/visual-studio-sdks?utm_source=getdotnetsdk&utm_medium=referral)

# Dependencies
- [WebP wrapper](https://github.com/JosePineiro/WebP-wrapper)
- [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [HtmlAgilityPack](https://html-agility-pack.net/)

# Versions
- [![release latest](https://img.shields.io/github/release/rmagur1203/HitomiViewer.svg?logo=github)](https://github.com/rmagur1203/HitomiViewer/releases/latest)
- [![3.8.1.4](https://img.shields.io/github/downloads/rmagur1203/HitomiViewer/3.8.1.4/total?logo=github)](https://github.com/rmagur1203/HitomiViewer/releases/tag/3.8.1.4)
- [![3.7.8](https://img.shields.io/github/downloads/rmagur1203/HitomiViewer/3.7.8/total?logo=github)](https://github.com/rmagur1203/HitomiViewer/releases/tag/3.7.8)
- [![3.6.6](https://img.shields.io/github/downloads/rmagur1203/HitomiViewer/3.6.6/total?logo=github)](https://github.com/rmagur1203/HitomiViewer/releases/tag/3.6.6)
- [![3.6.1](https://img.shields.io/github/downloads/rmagur1203/HitomiViewer/3.6.1/total?logo=github)](https://github.com/rmagur1203/HitomiViewer/releases/tag/3.6.1)
- [![3.5.1](https://img.shields.io/github/downloads/rmagur1203/HitomiViewer/3.5.1/total?logo=github)](https://github.com/rmagur1203/HitomiViewer/releases/tag/3.5.1)
- [![3.2](https://img.shields.io/github/downloads/rmagur1203/HitomiViewer/v3.2/total?logo=github)](https://github.com/rmagur1203/HitomiViewer/releases/tag/v3.2)

## Description
- 3.8.1.4
  - 업데이트 취소 추가
  - System.IndexOutOfRangeException 오류 수정
  - 히토미 검색시에 없는 태그를 넣을시 NotFound 뜨며 꺼지는 오류 수정
- 3.7.8
  - 안전한 데이터 저장 추가
  - 검색어 추천 기능 추가
- 3.7.6.3
- 3.7.6.2
  - 뷰어가 한번에 2개씩 켜지는 버그 수정됨.
  - 미리보기 이미지가 모두 블러처리가 되는 버그 수정됨.
  - 제외 태그(태그 차단) 추가
- 3.7.6
  - 뷰어가 한번에 2개씩 켜지는 버그 발생.
  - 미리보기 이미지가 모두 블러처리가 되는 버그 발생.
  - System.NullReferenceException 수정 됨.
- 3.7.5
  - 타이틀 암호화 추가
  - 랜덤 타이틀 추가
  - 다운로드에 쓸 폴더 지정 추가
- 3.7.4
  - 전반적인 코드 수정
  - 번호 목록 내보내기와 불러오기 추가
  - 이미지 로딩중에 현재 작품을 종료하고 다른 작품을 열 시에 System.IndexOutOfRangeException 오류 발생
- 3.7.3
  - 비밀번호 기능 추가
  - 히요비 검색 추가
  - Authors 에서 System.NullReferenceException 오류 발생
- 3.6.6
  - 히토미 추가
  - 다운로드시 태그도 같이 다운로드
- 3.6.1
  - 히요비 추가 (검색 없음)
- 3.5.1
  - 태그 표시
- 3.2
  - 초기 버전
  - 야간 모드
