# 내가 친구가 될 수 있을 리 없잖아 무리무리! (※무리가 아니었다?!)


<img width="1536" height="1024" alt="Image" src="https://github.com/user-attachments/assets/d0850bcc-3055-4409-9cec-0ab3cd101d2a" />

```
사이버 미소녀 '카나'와 두근두근 연애 생활...?!
```

---

|팀원|github|
|------|---|
|김창균|https://github.com/akeastshore|
|박승완|https://github.com/wanipark1004|
|유은선|https://github.com/Erc-nard|




### 💔 소개
<br>
우연히 시작한 게임에서 만나게 된 사이버 AI 미소녀 '카나'. 카나 덕에 여자와 대화할 수 있게 되어 좋았는데... 뭔가 이상하다?! (※심약자 플레이 주의, 점프스케어 장면이 있습니다.)
-

<br>


### 💔 게임 설명
### Scene. 메인

- 카나가 배경에서 움직임
- 시작 버튼을 누를 시 처음부터 시작
- 설정 탭에서 소리, 효과음 조절 가능
<br>


### Scene. 두근 두근 미연시
- 플레이어 이름을 입력받아 카나가 해당 이름으로 부를 수 있음
- 선택지에 따라 호감도 획득 → 호감도에 따라 배드/노말/트루엔딩을 볼 수 있음
- 미니게임을 통해 카나와 놀 수 있음
<br>

### Scene. 미니게임
### 참, 참, 참!

- 헤드 트래킹 상호작용
  - 실시간 머리 방향 감지: 카메라를 통해 플레이어의 머리 방향 추적 (왼쪽/중앙/오른쪽)
  - AI 공격 방향: 배경 영상과 동기화된 AI의 공격 방향 결정
  - 카운트다운: "준비..." → "참!" → "참!!" → "참!!!" 타이밍 시퀀스
  <br>
  <video src="https://github.com/user-attachments/assets/128a8057-7879-458e-85c3-f8f2dade0947" width="320" controls autoplay loop muted></video>
  <br>
- 공격 회피 시스템
  - AI 공격: 왼쪽(👈) / 오른쪽(👉) / 중앙(👇) 중 랜덤 선택
  - 회피 판정: 플레이어가 **AI와 다른 방향**을 보면 회피 성공 (승리)
  - 실패 판정: 플레이어가 **AI와 같은 방향**을 보면 공격 맞음 (패배)
- 1회 승부로 즉시 승패 결정, 승리 시 다음 스토리 진행, 패배 시 스토리 분기

### 눈빛 보내기

- 시선 추적 상호작용
  - 눈 커서: 실시간 시선 위치 추적으로 화면 속 타겟을 쫓아가는 인터페이스
  - 타겟 이동: 도망다니는 타겟을 일정 시간 이상 시선으로 고정해야 성공
  - 거리 판정: 눈 커서와 타겟 간 거리가 80px 이내일 때 히트 판정
  <br>
  <video src="https://github.com/user-attachments/assets/397bce41-eb03-4c32-a830-d603fd3906c6" width="320" controls autoplay loop muted></video>
  <br>
- 호감도 게이지 시스템
  - 시선 고정 성공 시 초당 **+0.5** 게이지 상승
  - 시선 이탈 시 초당 **-0.3** 게이지 하락
  - 게이지 0%~100% 실시간 변동
- 게이지 100% 도달 시 심쿵 성공, 시선 유지 실패 시 게이지 소진

### 카나와 리듬 놀이

- 리듬 게임 상호작용
  - 3레인 시스템: 화면을 3등분하여 각 레인마다 노트 생성
  - 노트 낙하: 상단에서 하단 판정선으로 떨어지는 노트를 타이밍에 맞춰 터치
  - 판정 범위: 노트가 판정선과 100px 이내에 있을 때 터치 시 성공
  <br>
  <video src="https://github.com/user-attachments/assets/6d733eaa-2e97-4c5e-9e30-fc54d547eb05" width="320" controls autoplay loop muted></video>
  <br>
- 점수 및 페널티 시스템
  - 노트 히트 시 **+10점**, 미스 시 **-20점** (최소 0점)
  - 1초마다 랜덤 레인에 노트 자동 생성
  - 판정선 통과 시 자동 미스 판정
- 목표 점수 300점 달성 시 성공

### Rebuild: 카나
- AI 시스템 복원 상호작용
  - 터미널 UI: 실시간 타이핑 효과를 통해 손상된 코드를 분석하고 정답 선택
  - 캐릭터 피드백: 상황(Normal, Correct, Wrong, Death)에 따른 실시간 영상 전환
  - 시각 연출: 정답 시 화이트 페이드 효과 및 게이지 위험 시 깜박임 효과
  <br>
  <video src="https://github.com/user-attachments/assets/1a235867-9b35-417a-a893-2718669f084c" width="320" controls autoplay loop muted></video>
  <br>
- 동기화 게이지 및 페널티
  - 정답 시 **+25%**, 오답 시 **-50%** (2회 오답 시 시스템 복구 실패)
  - 10초 경과 후부터 초당 **-5%** 게이지 페널티 지속 발생
- 모든 스테이지 클리어 시 복원 성공, 게이지 0% 도달 시 실패 및 자동 재시작
<br>

### 💘 개발 스택

- 개발 언어: C#
- 게임 엔진: Unity
- 그래픽: Figma, Vroid Studio, Blender, Aseprite

<br>


### 🟡 APK 파일

[여기](https://github.com/Erc-nard/murimuri/releases/download/v1.0/murimuri.apk)를 클릭하여 바로 내려받거나, Releases 탭에서 내역을 확인할 수 있습니다.
