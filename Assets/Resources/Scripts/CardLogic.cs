﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLogic : MonoBehaviour {
	public Card[] cardsArray;
	public int[] cardNumberArray = new int[Public.CARD_NUMS];
	public Deck[] bottomDeckArray = new Deck[7];
	public Deck[] aceDeckArray = new Deck[4];
	public Deck[] allDeckArray = new Deck[13]; 
	public Deck   wasteDeck;
	public Deck   packDeck; 
	public GameManager gameMgr;
	void Start () { 
		//初始化牌组
		InitCardNodes();
		//
		InitAllDeckArray();
		//cards 
		int k = 0;
		for (int i = 0; i < 7; i++)
		{
			for (int j = 0; j < i + 1; j++)
			{
				bottomDeckArray[i].PushCard(cardsArray[k]);  
				k++;
			}
			bottomDeckArray[i].UpdateCardsPosition(true);
		}
		for (int i = k; i < 51; i++) {
			packDeck.PushCard (cardsArray[i]);
		}
		packDeck.UpdateCardsPosition (true);
		//
		wasteDeck.PushCard(cardsArray[51]);
		wasteDeck.UpdateCardsPosition (true);
		// 
	}

	void Awake(){ 
		cardNumberArray = new int[Public.CARD_NUMS];
	}
	//随机生成牌的数组
	void GenerateRandomCardNums(){
		int[] tagArray = new int[Public.CARD_NUMS];
		int i = 0;
		for (i = 0; i < Public.CARD_NUMS; i++) {
			tagArray[i] = 0;
		} 
		int k = 0;
		for (i = 0; i < Public.CARD_NUMS; i++) {
			int rand = Random.Range (0, Public.CARD_NUMS);
			while (rand < Public.CARD_NUMS && tagArray[rand] == 1 ) {
				rand = Random.Range (0, Public.CARD_NUMS);
			}  
			tagArray[rand] = 1; 
			cardNumberArray[i] = rand;
			//Debug.Log (rand);
		} 
	}

	void InitCardNodes()
	{
		for (int i = 0; i < Public.CARD_NUMS; i++)
		{
			GameObject objPrefab = (GameObject)Resources.Load("Prefabs/card");
			objPrefab = Instantiate(objPrefab);
			objPrefab.transform.SetParent(wasteDeck.transform.parent); 
			cardsArray[i] = objPrefab.GetComponent<Card>();
			cardsArray [i].InitWithNumber (i);
			cardsArray [i].cardLogic = this;
		}
	}

	void InitAllDeckArray(){
		int j = 0;
		for (int i = 0; i < 4; i++) {
			aceDeckArray [i].deckType = Public.DECK_TYPE_ACE;
			allDeckArray [j++] = aceDeckArray[i];
		}
		for (int i = 0; i < 7; i++) {
			bottomDeckArray [i].deckType = Public.DECK_TYPE_BOTTOM;
			allDeckArray [j++] = bottomDeckArray[i];
		}
		wasteDeck.deckType = Public.DECK_TYPE_WASTE;
		allDeckArray [j++] = wasteDeck;
		packDeck.deckType = Public.DECK_TYPE_PACK;
		allDeckArray [j++] = packDeck;
	}

	//拖动结束，需要判断是否可以转移牌
	public void OnDragEnd(Card card){
		for (int i = 0; i < allDeckArray.Length; i++) {
			Deck targetDeck = allDeckArray [i];
			if (targetDeck.deckType == Public.DECK_TYPE_BOTTOM || targetDeck.deckType == Public.DECK_TYPE_ACE) {
				if (targetDeck.OverlapWithCard(card)) {    //优先判断ace牌桌
					Deck srcDeck = card.deck;
					Debug.Log ("Overlap with deck");
					if (targetDeck.AcceptCard(card)) {     //可以接收牌
						Card[] popCards = srcDeck.PopFromCard(card);
						targetDeck.PushCardArray (popCards);
						//targetDeck.PushCard(srcDeck.Pop());
						targetDeck.UpdateCardsPosition (false);
						srcDeck.UpdateCardsPosition (false);
						ActionAfterEachStep ();
						break;
					}
				}
			}
		}
	}

	//点击牌堆
	public void OnClickPack(){
		if (packDeck.GetCardNums () > 0) {
			wasteDeck.PushCard (packDeck.Pop ());
			packDeck.UpdateCardsPosition (false);
			wasteDeck.UpdateCardsPosition (false);
			ActionAfterEachStep ();
		} else {//需要重新回收牌
			if (wasteDeck.GetCardNums() > 0) {
				MoveWasteToPack();
				ActionAfterEachStep ();
			}
		}
	}
	//转移牌
	public void MoveWasteToPack(){
		int cardNums = wasteDeck.GetCardNums ();
		for (int i = 0; i < cardNums; i++) {
			packDeck.PushCard (wasteDeck.Pop());
		}
		packDeck.UpdateCardsPosition (false);
		wasteDeck.UpdateCardsPosition (false);
	}
	//判断是否赢了
	public void CheckWinGame(){
		bool hasWin = true;
		for (int i = 0; i < 4; i++) {
			if (aceDeckArray [i].GetCardNums () != 13) {
				hasWin = false;
				break;
			}
		}
		if (hasWin) {
			gameMgr.HasWinGame ();
		}
	}
	//每步之后的操作
	public void ActionAfterEachStep(){
		CheckWinGame ();   
		gameMgr.CardMove ();
	}
	//洗牌
	public void Shuffle(bool bReplay){
		if (!bReplay) {
			GenerateRandomCardNums ();
		}
	}
	//清空状态
	public void RestoreInitialState(){
		for (int i = 0; i < 13; i++) {
			allDeckArray [i].RestoreInitialState ();
		}
	}
}
